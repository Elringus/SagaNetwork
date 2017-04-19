using System;
using System.Collections;
using System.Security.Claims;
using System.Linq.Expressions;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Html;
using System.Text.Encodings.Web;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;

namespace SagaNetwork
{
    public static class ViewHelpers
    {
        /// <summary>
        /// Renders value editor for given property of standart type. 
        /// For custom types and collections use TypeEditor and ListEditor respectively.
        /// </summary>
        /// <param name="label">Custom label for the input field.</param>
        /// <param name="@readonly">If the input should be in readonly state.</param>
        /// <param name="readonly">If field should be read-only.</param>
        /// <param name="required">If the input is required to be filled before submitting the form.</param>
        /// <param name="defaultValue">Default value of the field.</param>
        public static IHtmlContent ValueEditorFor<TModel, TResult> (this IHtmlHelper<TModel> htmlHelper, 
            Expression<Func<TModel, TResult>> expression, bool @readonly = false, bool required = false, string label = null, dynamic defaultValue = null, Type expressionReturnType = null)
        {
            if (string.IsNullOrEmpty(label)) label = ExtractPropertyName(expression);
            var shortLabel = label.Truncate(20);
            var checkboxHack = expression.ReturnType == typeof(bool) ? @"style=""width:64px;""" : string.Empty;
            var tagBuilder = new TagBuilder("div");
            tagBuilder.AddCssClass("form-group");

            var attributes = new Dictionary<string, dynamic>();
            attributes.Add("class", "form-control");
            if (@readonly) attributes.Add("readonly", true);
            if (required) attributes.Add("required", true);
            if (defaultValue != null) attributes.Add("Value", defaultValue);

            tagBuilder.InnerHtml.AppendLine(htmlHelper.LabelFor(expression, shortLabel, new { @class = "col-md-2 control-label", title = label }));
            tagBuilder.InnerHtml.AppendHtmlLine($@"<div class=""col-md-10"" {checkboxHack}>");
            if (expression.ReturnType.IsEnum) tagBuilder.InnerHtml.AppendLine(htmlHelper.DropDownListFor(expression, htmlHelper.GetEnumSelectList(expression.ReturnType), null, attributes));
            else tagBuilder.InnerHtml.AppendLine(htmlHelper.EditorFor(expression, new { htmlAttributes = attributes }));
            if (!@readonly) tagBuilder.InnerHtml.AppendLine(htmlHelper.ValidationMessageFor(expression, null, new { @class = "label label-danger" }));
            tagBuilder.InnerHtml.AppendHtmlLine(@"</div>");

            return tagBuilder;
        }

        /// <summary>
        /// Renders value editor for given property of custom type. 
        /// Corresponding editor template should be located in Shared/TypeEditors/_{TypeName}TypeEditor.cshtml.
        /// </summary>
        public static IHtmlContent TypeEditorFor<TModel, TResult> (this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TResult>> expression, string label = null) 
        {
            var model = htmlHelper.ViewData.Model;
            var editedObject = expression.Compile().DynamicInvoke(model);
            var typeName = editedObject?.GetType().Name ?? "Null";
            var viewDataDictionary = new ViewDataDictionary(htmlHelper.ViewData);
            viewDataDictionary.InjectModelId(model);
            viewDataDictionary.ChainPropertyPath(expression);
            viewDataDictionary["Label"] = label ?? ExtractPropertyName(expression);

            return htmlHelper.Partial($"TypeEditors/_{typeName}TypeEditor", editedObject, viewDataDictionary);
        }

        /// <summary>
        /// Renders list editor for given property.
        /// Editor template for corresponding generic list type should be located in Shared/ListEditors/_{TypeName}ListEditor.cshtml.
        /// </summary>
        public static IHtmlContent ListEditorFor<TModel, TResult> (this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TResult>> expression, string label = null)
            where TResult : IList
        {
            var model = htmlHelper.ViewData.Model;
            var listObject = expression.Compile().DynamicInvoke(model);
            var viewDataDictionary = new ViewDataDictionary(htmlHelper.ViewData);
            viewDataDictionary.InjectModelId(model);
            viewDataDictionary.ChainPropertyPath(expression);
            viewDataDictionary["Label"] = label ?? ExtractPropertyName(expression);

            return htmlHelper.Partial("ListEditors/_ListEditorPartial", listObject, viewDataDictionary);
        }

        /// <summary>
        /// Randers key/value inputs for a dictionary(string, string) element and binds them to the model.
        /// </summary>
        /// <param name="dictionaryName">Name of the model property to bind with.</param>
        /// <param name="itemIndex">Index of the element.</param>
        /// <param name="initialValues">Initial values of key and value.</param>
        public static IHtmlContent DictionaryItemEditorFor (this IHtmlHelper htmlHelper, string dictionaryName, int itemIndex, KeyValuePair<string, string> initialValues, 
            dynamic predefinedKeys = null, dynamic predefinedValues = null)
        {
            var tagBuilder = new TagBuilder("div");
            tagBuilder.AddCssClass("form-group");

            tagBuilder.InnerHtml.AppendHtmlLine($@"<div class=""col-md-12"">");

            if (predefinedKeys != null && predefinedKeys.Count > 0)
            {
                tagBuilder.InnerHtml.AppendHtmlLine($@"<select name=""{dictionaryName}[{itemIndex}].Key"" class=""col-md-6"" style=""padding-top: 2.5px; padding-bottom: 2.5px;"">");
                foreach (var predefinedKey in predefinedKeys)
                {
                    if (predefinedKey == initialValues.Key)
                        tagBuilder.InnerHtml.AppendHtmlLine($@"<option selected=""selected"">{predefinedKey}</option>");
                    else tagBuilder.InnerHtml.AppendHtmlLine($@"<option>{predefinedKey}</option>");
                }
                tagBuilder.InnerHtml.AppendHtmlLine(@"</select>");
            }
            else tagBuilder.InnerHtml.AppendHtmlLine($@"<input name=""{dictionaryName}[{itemIndex}].Key"" value=""{initialValues.Key}"" class=""col-md-6"" />");

            if (predefinedValues != null && predefinedValues.Count > 0)
            {
                tagBuilder.InnerHtml.AppendHtmlLine($@"<select name=""{dictionaryName}[{itemIndex}].Value"" class=""col-md-6"" style=""padding-top: 2.5px; padding-bottom: 2.5px;"">");
                foreach (var predefinedValue in predefinedValues)
                {
                    if (predefinedValue == initialValues.Value)
                        tagBuilder.InnerHtml.AppendHtmlLine($@"<option selected=""selected"">{predefinedValue}</option>");
                    else tagBuilder.InnerHtml.AppendHtmlLine($@"<option>{predefinedValue}</option>");
                }
                tagBuilder.InnerHtml.AppendHtmlLine(@"</select>");
            }
            else tagBuilder.InnerHtml.AppendHtmlLine($@"<input name=""{dictionaryName}[{itemIndex}].Value"" value=""{initialValues.Value}"" class=""col-md-6"" />");

            tagBuilder.InnerHtml.AppendHtmlLine(@"</div>");

            return tagBuilder;
        }

        /// <summary>
        /// Renders a collapsable panel with a list inside.
        /// Use BeginCollapsableListElement to render list items.
        /// </summary>
        /// <param name="title">Text to show as panel title.</param>
        /// <param name="addItemController">Name of the controller for adding new list items.</param>
        /// <param name="additionalArguments">Additonal args for the controller.</param>
        public static IDisposable BeginCollapsableList (this IHtmlHelper htmlHelper, string title, 
            string addItemController = null, IDictionary<string, string> additionalArguments = null)
        {
            var collapseId = Guid.NewGuid().ToString("N");
            htmlHelper.ViewContext.Writer.WriteLine(
                $@"
                    <div class=""panel panel-default"">
                        <div class=""panel-heading"">
                            <h4 class=""panel-title"">
                                <a data-toggle=""collapse"" data-target=""#{collapseId}"" href=""#{collapseId}"" class=""collapsed"">{title}</a>
                            </h4>
                        </div>
                        <div id=""{collapseId}"" class=""panel-collapse collapse"">
                            <ul class=""list-group"">
                "
            );

            var closingContent = @"</ul></div></div>";
            if (addItemController != null)
            {
                closingContent = closingContent.Insert(0,
                    $@"
                        <li class=""list-group-item"">
                            {htmlHelper.Partial("_AddItemPartial", new ViewDataDictionary(htmlHelper.MetadataProvider, htmlHelper.ViewContext.ModelState) {
                                ["BeginCollapsableList_AddItemController"] = addItemController,
                                ["BeginCollapsableList_AdditionalArguments"] = additionalArguments,
                                ["BeginCollapsableList_AddItemButtonLabel"] = $"Add item to {title.ToLower()}",
                            }).AsString()}
                        </li>
                    "
                );
            }

            return new EnclosedContent(htmlHelper, closingContent);
        }

        /// <summary>
        /// Renders a collapsable list item.
        /// Should be used within BeginCollapsableList body.
        /// </summary>
        /// <param name="removeItemController">Name of the controller to remove the item from the list.</param>
        /// <param name="additionalArguments">Additonal args for the controller.</param>
        public static IDisposable BeginCollapsableListElement (this IHtmlHelper htmlHelper,
            string removeItemController = null, IDictionary<string, string> additionalArguments = null)
        {
            htmlHelper.ViewContext.Writer.WriteLine(@"<li class=""list-group-item"">");

            var closingContent = @"</li>";
            if (removeItemController != null)
            {
                closingContent = closingContent.Insert(0, htmlHelper.Partial("_RemoveItemPartial", 
                    new ViewDataDictionary(htmlHelper.MetadataProvider, htmlHelper.ViewContext.ModelState)
                    {
                        ["BeginCollapsableListElement_RemoveItemController"] = removeItemController,
                        ["BeginCollapsableListElement_AdditionalArguments"] = additionalArguments
                    }).AsString()
                );
            }

            return new EnclosedContent(htmlHelper, closingContent);
        }

        /// <summary>
        /// Renders a button to invoke editor for specific entity.
        /// <param name="entityId">ID of the entity to edit.</param>
        /// </summary>
        public static IHtmlContent RenderEditEntityButton (this IHtmlHelper htmlHelper, string entityId)
        {
            return htmlHelper.Partial("_EditEntityPartial", 
                new ViewDataDictionary(htmlHelper.MetadataProvider, htmlHelper.ViewContext.ModelState)
            {
                ["RenderEditEntityButton_EntityID"] = entityId
            });
        }

        /// <summary>
        /// Outputs html content as string.
        /// </summary>
        public static string AsString (this IHtmlContent content)
        {
            var writer = new System.IO.StringWriter();
            content.WriteTo(writer, HtmlEncoder.Create());
            return writer.ToString();
        }

        /// <summary>
        /// Outputs a user-friendly name of the claims type.
        /// </summary>
        public static string GetFriendlyName (this Claim claim)
        {
            var claimType = claim.Type;

            var lastSlashIndex = claimType.LastIndexOf('/');
            if (lastSlashIndex >= 0)
                claimType = claimType.Remove(0, lastSlashIndex + 1);

            claimType = claimType[0].ToString().ToUpper() + claimType.Substring(1);

            return claimType;
        }

        public static string Truncate (this string value, int maxChars)
        {
            return value.Length <= maxChars ? value : value.Substring(0, maxChars) + "..";
        }

        public static string TruncateTail (this string value, int maxChars)
        {
            if (value.Length <= maxChars)
                return value;

            return ".." + value.Substring(value.Length - maxChars);
        }

        /// <summary>
        /// Returns string after last found dot character.
        /// </summary>
        public static string AfterLastDot (this string value)
        {
            var lastDotIndex = value.LastIndexOf('.');
            if (lastDotIndex < 0) return value;

            return value.Substring(lastDotIndex).Replace(".", string.Empty);
        }

        /// <summary>
        /// Converts first char of the string to upper case.
        /// </summary>
        public static string FirstToUpper (this string value)
        {
            return value.First().ToString().ToUpper() + value.Substring(1);
        }

        /// <summary>
        /// Converts first char of the string to lower case.
        /// </summary>
        public static string FirstToLower (this string value)
        {
            return value.First().ToString().ToLower() + value.Substring(1);
        }

        private class EnclosedContent : IDisposable
        {
            private readonly IHtmlHelper htmlHelper;
            private readonly string closingContent;

            public EnclosedContent (IHtmlHelper htmlHelper, string closingContent = null)
            {
                this.htmlHelper = htmlHelper;
                this.closingContent = closingContent;
            }

            public void Dispose ()
            {
                if (closingContent != null)
                    htmlHelper.ViewContext.Writer.WriteLine(closingContent);
            }
        }

        private static void InjectModelId (this ViewDataDictionary viewDataDictionary, object model)
        {
            if ((model as TableEntity) != null)
            {
                var tableModel = model as TableEntity;
                viewDataDictionary.AddUnique("EntityId", tableModel.PartitionKey);
            }
        }

        private static void ChainPropertyPath (this ViewDataDictionary viewDataDictionary, LambdaExpression expression)
        {
            var extractedPropertyPath = ExtractPropertyPath(expression);

            if (viewDataDictionary.ContainsKey("PropertyPath"))
            {
                if (extractedPropertyPath.First() != '[') viewDataDictionary["PropertyPath"] += ".";
                viewDataDictionary["PropertyPath"] += extractedPropertyPath;
            }
            else viewDataDictionary["PropertyPath"] = extractedPropertyPath;

            viewDataDictionary.TemplateInfo.HtmlFieldPrefix = viewDataDictionary["PropertyPath"] as string;
        }

        private static string ExtractPropertyName (LambdaExpression expression)
        {
            var memberExpression = expression.Body as MemberExpression ?? (expression.Body as MethodCallExpression).Object as MemberExpression;
            return memberExpression?.Member?.Name;
        }

        private static string ExtractPropertyPath (LambdaExpression expression)
        {
            if (expression.Body is MemberExpression memberExpression)
                return memberExpression.Member.Name;

            var memberAccessExpression = expression.Body as MethodCallExpression;
            var memberIndex = Expression.Lambda(memberAccessExpression.Arguments[0]).Compile().DynamicInvoke();
            var listMemberExpression = memberAccessExpression.Object as MemberExpression;
            if (listMemberExpression == null) return $"[{memberIndex}]";
            var listMemberName = listMemberExpression.Member.Name;

            return $"{listMemberName}[{memberIndex}]";
        }
    }
}
