using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace si.birokrat.next.common_mvc.errors {
    public static class ModelStateExtensions {
        public static string FindFirstModelStateError(this ControllerBase controller) {
            return controller.ModelState.Values.SelectMany(value => value.Errors).Select(error => error.ErrorMessage).FirstOrDefault();
        }
    }
}
