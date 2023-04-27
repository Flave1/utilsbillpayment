using PagedList;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using VendTech.DAL;

namespace VendTech.BLL.Models
{
    public class ModelUtils
    {
        public const string DISPLAY_DATE_FORMAT = "dd/MM/yyyy HH:mm:ss";

        public static List<SelectListItem> CloneSelectItemList(List<SelectListItem> toBeCloned)
        {
            if (toBeCloned != null)
            {
                List<SelectListItem> clone = new List<SelectListItem>(toBeCloned.Count);
                toBeCloned.ForEach(s => clone.Add( CloneSelectListItem(s) ));
                return clone;
            }

            return new List<SelectListItem>(0);
        }

        public static List<SelectListItem> GetStatusEnumSelectItemList()
        {
            IEnumerable<StatusEnum> enums = EnumUtils.GetEnumValues<StatusEnum>();
            List<SelectListItem> result = new List<SelectListItem>(enums.Count());
            
            foreach (StatusEnum s in enums)
            {
                result.Add(new SelectListItem { Value = ((int) s).ToString(), Text = s.ToString() });
            }
            return result;
        }

        public static List<SelectListItem> GetTransactionStatusEnumSelectItemList()
        {
            IEnumerable<TransactionStatus> enums = EnumUtils.GetEnumValues<TransactionStatus>();
            List<SelectListItem> result = new List<SelectListItem>(enums.Count());

            foreach (TransactionStatus s in enums)
            {
                result.Add(new SelectListItem { Value = ((int)s).ToString(), Text = s.ToString() });
            }
            return result;
        }

        public static List<SelectListItem> GetCurrencySelectItemList(ICollection<Currency> currencies)
        { 
            if (currencies != null && currencies.Count > 0)
            {
                List<SelectListItem> result = new List<SelectListItem>(currencies.Count());
                foreach (Currency c in currencies)
                {
                    result.Add(new SelectListItem { Value = (c.Id).ToString(), Text = c.Name });
                }
                return result;
            }

            return new List<SelectListItem>(0);
        }

        public static string CreateSelectList(
            string name, 
            List<SelectListItem> options, 
            string currentValue = "", 
            string cssClass = "",
            string cssStyle = "",
            string defaultOptionText = " -- ", 
            string defaultOptionValue = "")
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("'name' for the <select> field cannot be null or empty");
            }

            StringBuilder html = new StringBuilder("<select name='").Append(name).Append("' class='")
                .Append(cssClass ?? "").Append("' style='").Append(cssStyle ?? "").Append("'>");

            if (!string.IsNullOrWhiteSpace(defaultOptionText))
            {
                html.Append(CreateOptionTag(defaultOptionText, defaultOptionValue ?? "", currentValue ?? ""));
            }

            if (options != null)
            {
                foreach (SelectListItem item in options)
                {
                    html.Append(CreateOptionTag(item.Text, item.Value, currentValue ?? ""));
                }
            }

            html.Append("</select>");

            return html.ToString();
        }

        private static string CreateOptionTag(string text, string value, string currentValue)
        {
            return "<option value='" + value + "'" + ((currentValue == value) ? " selected" : "") + ">" + text + "</option>";
        }

        public static SelectListItem CloneSelectListItem(SelectListItem s)
        {
            return new SelectListItem
            {
                Text = s.Text,
                Value = s.Value,
                Disabled = s.Disabled,
                Group = s.Group,
                Selected = s.Selected
            };
        }
    }

    public static class DateTimeUtilsExtension
    {
        public static DateTime ResetTimeToStartOfDay(this DateTime dateTime)
        {
            return new DateTime(
            dateTime.Year,
            dateTime.Month,
            dateTime.Day,
            0, 0, 0, 0);
        }
        public static DateTime ResetTimeToEndOfDay(this DateTime dateTime)
        {
            return new DateTime(
            dateTime.Year,
            dateTime.Month,
            dateTime.Day,
            23, 59, 59, 999);
        }
    }

    public class DataQueryModel
    {
        public readonly static int DEFAULT_PAGE_SIZE = 15;
        public readonly static string DATE_TIME_PICKER_FORMAT = "dd/MM/yyyy HH:mm";

        public int Page { get; set; }
        public int PageSize { get; set; }
        public long UserId { get; set; }
        public int PlatformId { get; set; }
        public bool IsAdmin { get; set; }
        public string Reference { get; set; }
        public string Beneficiary { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime FromDate { get; set; }
        public int ProductId { get; set; }
        public int Status { get; set; }
        public int ApiConnId { get; set; }
        public string FromDateFormatted
        {
            get { return FromDate.ToString(DATE_TIME_PICKER_FORMAT); }
        }
        public string ToDateFormatted
        {
            get { return ToDate.ToString(DATE_TIME_PICKER_FORMAT); }
        }

        public bool IsUserCheck
        {
            get { return UserId > 0; }
        }

        public static DataQueryModel New()
        {
            return new DataQueryModel
            {
                PageSize = DEFAULT_PAGE_SIZE,
                Page = 1,
            };
        }
    }

    public class DataTableResultModel<T> : DataQueryModel
    {
        public long TotalCount;

        public IPagedList<T> PagedList { get; set; }

        public List<T> Results { get; set; }

        public static DataTableResultModel<T> NewResultModel()
        {
            return new DataTableResultModel<T>();
        }
    }

    /**
     * This class is to be used for HTML form elements that can 
     * be used for collecting data i.e <select> and <input> tags.
     */
    public class HtmlFormElement
    {

        private readonly HashSet<String> CssClasses = new HashSet<string>();
        private readonly IDictionary<string, string> Options = new Dictionary<string, string>();
        private IDictionary<string, string> ExtraAttributes = new Dictionary<string, string>();

        public string Value { get; set; }
        public string Tag { get; }
        public string Field { get; }
        public string Name  { get; } //Name that is used with labels for the element

        private bool IsInputField = false;
        public string Style { get; set; }
        public string ExtraHtml { get; set; }
        public string AppendHtml { get; set; } //Extra HTML that will be appended to the generated HTML
        

        public HtmlFormElement(string _name, string _tag, string _field, string _value)
        {
            if (string.IsNullOrWhiteSpace(_name))
            {
                throw new ArgumentException("Name of the element to use for labeling cannot be null or empty");
            }

            if (string.IsNullOrWhiteSpace(_tag))
            {
                throw new ArgumentException("HTML Tag cannot be null or empty");
            }

            if (string.IsNullOrWhiteSpace(_field))
            {
                throw new ArgumentException("Field name (used as the 'name' attribute) cannot be null or empty");
            }

            //TODO: check if the tag is a valid one
            this.Name = _name;
            this.Tag = _tag.ToLower();
            this.Field = _field;
            this.Value = _value ?? "";

            if (Tag.Contains("input"))
            {
                IsInputField = true;
            }
            
        }

        public string GenerateHtml()
        {
            StringBuilder html = new StringBuilder("<").Append(Tag)
                            .Append(" name='").Append(Field).Append("'")
                            .Append(" style='").Append(Style ?? "").Append("'")
                            .Append(" class='");
           
            if (CssClasses.Count > 0)
            {
                foreach(string cssClass in CssClasses)
                {
                    html.Append(" ").Append(cssClass);
                }
            }

            html.Append("'");

            AddExtras(html);

            switch(Tag)
            {
                case "input":
                    html.Append(" value='").Append(Value).Append("' />");
                    break;
                case "span":
                    html.Append(">").Append(Value).Append("</span>");
                    break;
                case "select":
                    html.Append(">");
                    foreach (var kvp in Options)
                    {
                        html.Append("<option value='").Append(kvp.Value).Append("'");

                        if (Value == kvp.Value)
                        {
                            html.Append(" selected");
                        }

                        html.Append(">").Append(kvp.Key).Append("</option>");
                    }
                    html.Append("</").Append(Tag).Append(">");
                    break;
                default:
                    break;
            }

            html.Append(AppendHtml ?? "");

            return html.ToString();
        }

        private StringBuilder AddExtras(StringBuilder html)
        {
            foreach (var kvp in ExtraAttributes)
            {
                html.Append(" ").Append(kvp.Key).Append("='").Append(kvp.Value).Append("'");
            }

            html.Append(! string.IsNullOrWhiteSpace(ExtraHtml) ? " " + ExtraHtml : "");

            return html;
        }

        public HtmlFormElement AddCssClass(string cssClass)
        {
            if ( ! string.IsNullOrWhiteSpace(cssClass ))
            {
                CssClasses.Add(cssClass);
            }

            return this;
        }

        public HtmlFormElement AddAttribute(string name, string value)
        {
            if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(value))
            {
                ExtraAttributes.Add(name, value);
            }

            return this;
        }

        /**
         * Add an <option> tag for a select element.
         * The name parameter is the display name of the option
         */
        public bool AddOption(string name, string value)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Option name cannot be null");
            }

            if (Options.ContainsKey(name))
            {
                Options.Remove(name);
            }

            Options.Add(name, value ?? "");

            return true;
        }

        public void AddOptions(IDictionary<string, string> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("Options cannot be null");
            }

            foreach(var kvp in options)
            {
                if (string.IsNullOrWhiteSpace(kvp.Key))
                {
                    throw new ArgumentException("Key in dictionary to use for options is null. Ensure that all keys are not null");
                }

                AddOption(kvp.Key, kvp.Value);
            }
        }
    }
}
