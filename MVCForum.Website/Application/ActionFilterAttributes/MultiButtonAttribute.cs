using System;
using System.Linq;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Reflection;

namespace MVCForum.Website.Application.ActionFilterAttributes
{
    public class BasicMultiButtonAttribute : ActionNameSelectorAttribute
    {
        public string Name { get; set; }
      
        public BasicMultiButtonAttribute(string name)
        {
            this.Name = name;
        }
        public override bool IsValidName(ControllerContext controllerContext,
            string actionName, System.Reflection.MethodInfo methodInfo)
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                return false;
            }
            return controllerContext.HttpContext.Request.Form.AllKeys.Contains(this.Name);
        }
    }


//    Controller：
//    [HttpPost]
//    [MultiButton(Name = "delete", Argument = "id")]
//    public ActionResult Delete(string id)
//    {
//        var response = System.Web.HttpContext.Current.Response;
//        response.Write("Delete action was invoked with " + id);
//        return View();
//    }

//View:
//    <input type = "submit" value="not important" name="delete" /> 
//    <input type = "submit" value="not important" name="delete:id" />

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class MultiButtonAttribute : ActionNameSelectorAttribute
    {
        public string Name { get; set; }
        public string Argument { get; set; }

        public override bool IsValidName(ControllerContext controllerContext, string actionName, MethodInfo methodInfo)
        {
            var key = ButtonKeyFrom(controllerContext);
            var keyIsValid = IsValid(key);
            if (keyIsValid)
            {
                UpdateValueProviderIn(controllerContext, ValueFrom(key));
            }
            return keyIsValid;
        }

        private string ButtonKeyFrom(ControllerContext controllerContext)
        {
            var keys = controllerContext.HttpContext.Request.Params.AllKeys;
            return keys.FirstOrDefault(KeyStartsWithButtonName);
        }

        private static bool IsValid(string key)
        {
            return key != null;
        }

        private static string ValueFrom(string key)
        {
            var parts = key.Split(":".ToCharArray());
            return parts.Length < 2 ? null : parts[1];
        }

        private void UpdateValueProviderIn(ControllerContext controllerContext, string value)
        {
            //if (string.IsNullOrEmpty(Argument)) return;
            //controllerContext.Controller.ValueProvider[Argument] = new ValueProviderResult(value, value, null);

            if (string.IsNullOrEmpty(Argument)) return;
            controllerContext.RouteData.Values[this.Argument] = value;
        }

        private bool KeyStartsWithButtonName(string key)
        {
            return key.StartsWith(Name, StringComparison.InvariantCultureIgnoreCase);
        }

    }
}