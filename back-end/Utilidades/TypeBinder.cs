using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;

namespace back_end.Utilidades
{
    public class TypeBinder<T>: IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext modelBindingContext)
        {
            var nombrePropiedad = modelBindingContext.ModelName;
            var valor = modelBindingContext.ValueProvider.GetValue(nombrePropiedad);

            if(valor == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }


            try
            {
                var valorDeserializado = JsonConvert.DeserializeObject<T>(valor.FirstValue);
                modelBindingContext.Result = ModelBindingResult.Success(valorDeserializado);
            }
            catch
            {
                modelBindingContext.ModelState.TryAddModelError(nombrePropiedad, "El valor dado no es del tipo adecuado");
            }

            return Task.CompletedTask;
        }
    }
}
