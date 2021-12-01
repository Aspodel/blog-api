using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogApi.Services
{
    public class MultipleSourcesModelBinder<T> : IModelBinder where T : class
    {
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var body = bindingContext.HttpContext.Request.Body;
            using var reader = new StreamReader(body, Encoding.UTF8);
            var valueFromBody = await reader.ReadToEndAsync();
            var model = JsonConvert.DeserializeObject<T>(valueFromBody);

            var properties = typeof(T).GetProperties();
            foreach (var property in properties)
                if (property.CustomAttributes.Any(a => a.AttributeType == typeof(FromRouteAttribute)))
                {
                    var valueProvider = bindingContext.ValueProvider.GetValue(property.Name);
                    if (!string.IsNullOrEmpty(valueProvider.FirstValue))
                    {
                        property.SetValue(model, property.PropertyType switch
                        {
                            var t when t == typeof(int) => Convert.ToInt32(valueProvider.FirstValue),
                            _ => valueProvider.FirstValue
                        });
                    }

                }

            bindingContext.Result = ModelBindingResult.Success(model);
        }
    }
}
