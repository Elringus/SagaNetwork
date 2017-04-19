using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SagaNetwork.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SagaNetwork.Controllers
{
    /// <summary>
    /// Base MVC controller for all the editors. Provides basic editing methods.
    /// </summary>
    /// <typeparam name="T">Type of the entity to make editor for.</typeparam>
    [Authorize]
    public abstract class BaseEditorController<T> : Controller where T : TableModel<T>, new()
    {
        private const int BROWSE_LIMIT = 200;
        private const string ERROR_VIEW_PATH = "~/Views/Shared/Error.cshtml";

        #region Navigation and editing

        public virtual async Task<IActionResult> Browse (string entityId = null)
        {
            var entities = new List<T>();

            if (entityId == null)
            {
                entities = await new T().RetrieveAllAsync(BROWSE_LIMIT);
                if (entities.Count == BROWSE_LIMIT)
                    ViewBag.Limit = $"Shown first {BROWSE_LIMIT} records";
            }
            else
            {
                var entity = await LoadEntityAsync(entityId);
                if (entity != null)
                    entities.Add(entity);
                else ViewBag.NotFound = $"Entity with ID '{entityId}' not found in the {Configuration.DeploymentTier} DB";

                ViewBag.SearchId = entityId;
            }

            return View(entities);
        }

        public virtual async Task<IActionResult> Add (string entityId)
        {
            var entity = new T();
            entity.Id = entityId;

            if (!await entity.InsertAsync())
                return ErrorView("Entity with the specified ID already exists.");

            return RedirectToAction("Edit", new { entityId = entity.Id });
        }

        public virtual async Task<IActionResult> Edit (string entityId)
        {
            var entity = await LoadEntityAsync(entityId);

            if (entity == null)
                return ErrorView();

            var attributeMeta = await new EditorAttributeMeta(entity.FullTableName).LoadAsync();
            if (attributeMeta == null)
            {
                attributeMeta = new EditorAttributeMeta(entity.FullTableName);
                await attributeMeta.InsertAsync();
            }
            ViewData.Add("PredefinedAttributeKeys", attributeMeta.PredefinedKeys);
            ViewData.Add("PredefinedAttributeValues", attributeMeta.PredefinedValues);

            return View(entity);
        }

        public virtual async Task<IActionResult> Delete (string entityId)
        {
            var entity = await LoadEntityAsync(entityId);

            if (entity == null)
                return ErrorView();

            if (!await entity.DeleteAsync())
                return ErrorView();

            return RedirectToAction("Browse");
        }

        [HttpPost]
        public virtual async Task<IActionResult> Save (T entity)
        {
            if (!ModelState.IsValid) return View("Edit", entity);

            #region Form to model data binding 
            // Not using default model binder, as it's unstable (randomly skips collection values).
            var dictItem = new string[2] { null, null };
            foreach (var formItem in Request.Form)
            {
                var path = formItem.Key;
                var value = formItem.Value.First();

                if (path == "__RequestVerificationToken") continue;
                if (path.EndsWith("].Key") && ReflectionUtils.GetPropertyInfoByPath(entity, path.Substring(0, path.LastIndexOf('['))).IsGenericDict())
                {
                    dictItem[0] = value;
                    if (dictItem[1] == null) continue;
                }
                if (path.EndsWith("].Value") && ReflectionUtils.GetPropertyInfoByPath(entity, path.Substring(0, path.LastIndexOf('['))).IsGenericDict())
                {
                    dictItem[1] = value;
                    if (dictItem[0] == null) continue;
                }
                if (dictItem[0] != null && dictItem[1] != null)
                {
                    path = path.Substring(0, path.LastIndexOf('[') + 1) + dictItem[0] + "]";
                    value = dictItem[1];
                    dictItem[0] = null;
                    dictItem[1] = null;
                }

                var isValueSet = ReflectionUtils.SetPropertyByPath(entity, path, value);

                if (!isValueSet) return ErrorView($"Failed to assign value '{value}' to property {path} via reflection.");
            }
            #endregion

            if (!await entity.ReplaceAsync())
                return ErrorView();

            return RedirectToAction("Edit", new { entityId = entity.Id });
        }

        [HttpPost]
        public virtual async Task<IActionResult> Duplicate (string sourceEntityId, string newEntityId)
        {
            var sourceEntity = new T();
            sourceEntity.Id = sourceEntityId;
            sourceEntity = await sourceEntity.LoadAsync();

            if (sourceEntity == null)
                return ErrorView("Can't find source entity with specified ID.");

            var newEntity = new T();
            newEntity = sourceEntity;
            newEntity.Id = newEntityId;

            if (!await newEntity.InsertAsync())
                return ErrorView("Specified ID for the new entity is already used.");

            return RedirectToAction("Edit", new { entityId = newEntity.Id });
        }

        #endregion

        #region Attributes

        public virtual async Task<IActionResult> AddAttribute (string entityId)
        {
            var entity = await LoadEntityAsync(entityId);

            if (entity == null)
                return ErrorView();

            entity.Attributes.Add($"NULL#{Guid.NewGuid()}", "NULL");
            if (!await entity.ReplaceAsync())
                return ErrorView();

            return RedirectToAction("Edit", new { entityId = entity.Id });
        }

        public virtual async Task<IActionResult> RemoveAttribute (string entityId, int attributeIndex)
        {
            var entity = await LoadEntityAsync(entityId);

            if (entity == null || entity.Attributes == null ||
                entity.Attributes.Count <= attributeIndex)
                return ErrorView();

            entity.Attributes.Remove(entity.Attributes.ElementAt(attributeIndex).Key);
            if (!await entity.ReplaceAsync())
                return ErrorView();

            return RedirectToAction("Edit", new { entityId = entity.Id });
        }

        #endregion

        #region Lists
        public async Task<IActionResult> AddItemToList (string entityId, string listPropertyPath)
        {
            var entity = await LoadEntityAsync(entityId);
            if (entity == null)
                return ErrorView();

            var listObject = ReflectionUtils.GetPropertyValueByPath(entity, listPropertyPath) as IList;
            if (listObject == null)
                return ErrorView($"Wasn't able to get list property '{listPropertyPath}' via reflection.");
            var listGenericType = listObject.GetType().GetGenericArguments()[0];
            var newListValue = listGenericType == typeof(string) ? "" : Activator.CreateInstance(listGenericType);
            listObject.Add(newListValue);

            if (!await entity.ReplaceAsync())
                return ErrorView();

            return RedirectToAction("Edit", new { entityId = entity.Id });
        }

        public async Task<IActionResult> RemoveItemFromList (string entityId, int itemIndex, string listPropertyPath)
        {
            var entity = await LoadEntityAsync(entityId);
            if (entity == null)
                return ErrorView();

            var listObject = ReflectionUtils.GetPropertyValueByPath(entity, listPropertyPath) as IList;
            if (listObject == null)
                return ErrorView($"Wasn't able to get range list property '{listPropertyPath}' via reflection.");

            if (listObject.Count <= itemIndex)
                return ErrorView();

            listObject.RemoveAt(itemIndex);
            if (!await entity.ReplaceAsync())
                return ErrorView();

            return RedirectToAction("Edit", new { entityId = entity.Id });
        }
        #endregion

        #region Objects 
        public async Task<IActionResult> CreateObject (string entityId, string objectPropertyPath)
        {
            var entity = await LoadEntityAsync(entityId);
            if (entity == null)
                return ErrorView();

            var propertyInfo = ReflectionUtils.GetPropertyInfoByPath(entity, objectPropertyPath);
            var propertyHost = ReflectionUtils.GetPropertyHostByPath(entity, objectPropertyPath);
            if (propertyInfo == null)
                return ErrorView($"Wasn't able to get object property '{objectPropertyPath}' via reflection.");
            propertyInfo.SetValue(propertyHost, Activator.CreateInstance(propertyInfo.PropertyType));

            if (!await entity.ReplaceAsync())
                return ErrorView();

            return RedirectToAction("Edit", new { entityId = entity.Id });
        }

        public async Task<IActionResult> DestroyObject (string entityId, string objectPropertyPath)
        {
            var entity = await LoadEntityAsync(entityId);
            if (entity == null)
                return ErrorView();

            var propertyInfo = ReflectionUtils.GetPropertyInfoByPath(entity, objectPropertyPath);
            var propertyHost = ReflectionUtils.GetPropertyHostByPath(entity, objectPropertyPath);
            if (propertyInfo == null)
                return ErrorView($"Wasn't able to get object property '{objectPropertyPath}' via reflection.");
            propertyInfo.SetValue(propertyHost, null);

            if (!await entity.ReplaceAsync())
                return ErrorView();

            return RedirectToAction("Edit", new { entityId = entity.Id });
        }
        #endregion

        protected async Task<T> LoadEntityAsync (string entityId)
        {
            var entity = new T();
            entity.Id = entityId;
            return await entity.LoadAsync();
        }

        /// <summary>
        /// Renders custom error page with an optional error description if provided.
        /// </summary>
        protected ViewResult ErrorView (string errorDescription = null)
        {
            ViewBag.ErrorDescription = errorDescription;
            return View(ERROR_VIEW_PATH);
        }
    }
}
