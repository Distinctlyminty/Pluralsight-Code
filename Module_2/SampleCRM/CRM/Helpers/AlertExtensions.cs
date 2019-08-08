using System;
using System.Web.Mvc;

namespace CRM.Helpers
{
    public static class AlertExtensions
    {
        public static ActionResult WithSuccess(this ActionResult result, string title, string body)
        {
            return Alert(result, "success", title, body);
        }

        public static ActionResult WithInfo(this ActionResult result, string title, string body)
        {
            return Alert(result, "info", title, body);
        }

        public static ActionResult WithWarning(this ActionResult result, string title, string body)
        {
            return Alert(result, "warning", title, body);
        }

        public static ActionResult WithDanger(this ActionResult result, string title, string body)
        {
            return Alert(result, "danger", title, body);
        }

        private static ActionResult Alert(ActionResult result, string type, string title, string body)
        {
            if (result.GetType() == typeof(ViewResult))
            {
                ViewResult temp = (ViewResult)result;
                temp.TempData["_alert.type"] = type;
                temp.TempData["_alert.title"] = title;
                temp.TempData["_alert.body"] = body;
                return result;
            }
           
            else
            {
                return result;
            }
      
        }
    }
}
