using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace MVCForum.Website.Application.ActionFilterAttributes
{

    /// <summary>
    /// 时间先后验证
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public class DateTimeNotLessThan : ValidationAttribute, IClientValidatable
    {
        private const string DefaultErrorMessage = "{0} 不得早于 {1}.";
        public string OtherProperty { get; private set; }
        private string OtherPropertyName { get; set; }

        public DateTimeNotLessThan(string otherProperty, string otherPropertyName)
            : base(DefaultErrorMessage)
        {
            if (string.IsNullOrEmpty(otherProperty))
            {
                throw new ArgumentNullException("otherProperty");
            }

            OtherProperty = otherProperty;
            OtherPropertyName = otherPropertyName;
        }

        public override string FormatErrorMessage(string name)
        {
            return string.Format(ErrorMessageString, name, OtherPropertyName);
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value != null)
            {
                //DateTime dtNow = DateTime.Now;
                var otherProperty = validationContext.ObjectInstance.GetType().GetProperty(OtherProperty);
                if (otherProperty != null)
                {
                    var otherPropertyValue = otherProperty.GetValue(validationContext.ObjectInstance, null);
                    DateTime dtThis = Convert.ToDateTime(value);
                    DateTime dtOther = Convert.ToDateTime(otherPropertyValue);
                    if (dtThis < dtOther)
                    {
                        return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
                    }
                    //if (dtThis > dtNow)
                    //{
                    //    return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
                    //}
                }
            }
            return ValidationResult.Success;
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(
                                                      ModelMetadata metadata,
                                                      ControllerContext context)
        {
            var clientValidationRule = new ModelClientValidationRule()
            {
                ErrorMessage = FormatErrorMessage(metadata.GetDisplayName()),
                ValidationType = "notlessequalthan"//这里是核心点
            };

            clientValidationRule.ValidationParameters.Add("otherproperty", OtherProperty);
            return new[] { clientValidationRule };
        }
    }


    public class IsIntegerAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value != null)
            {
                int val;
                var isNumeric = int.TryParse(value.ToString(), out val);

                if (!isNumeric)
                {
                    return new ValidationResult("请输入一个整数数据。");
                }
            }

            return ValidationResult.Success;
        }
    }

}


namespace MVCForum.Website.Application.ActionFilterAttributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class IntegerAttribute : DataTypeAttribute
    {
        public IntegerAttribute()
            : base("integer")
        {
        }

        public override string FormatErrorMessage(string name)
        {
            if (ErrorMessage == null && ErrorMessageResourceName == null)
            {
                ErrorMessage ="格式无效。";
            }

            return base.FormatErrorMessage(name);
        }

        public override bool IsValid(object value)
        {
            if (value == null) return true;

            int retNum;

            return int.TryParse(Convert.ToString(value), out retNum);
        }
    }
}