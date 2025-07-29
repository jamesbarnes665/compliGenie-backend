using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Linq;

namespace CompliGenie.Conventions
{
    public class ExcludeTestControllersConvention : IApplicationModelConvention
    {
        private readonly string[] _testControllers = new[] 
        { 
            "DbTestController", 
            "NoAuthTestController", 
            "SetupController" 
        };

        public void Apply(ApplicationModel application)
        {
            var controllersToRemove = application.Controllers
                .Where(c => _testControllers.Contains(c.ControllerName))
                .ToList();

            foreach (var controller in controllersToRemove)
            {
                application.Controllers.Remove(controller);
            }
        }
    }
}