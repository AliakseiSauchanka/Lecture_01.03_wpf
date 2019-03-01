using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace WPF.Example
{
    public class MainViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private readonly Dictionary<string, List<string>> _validationResults = new Dictionary<string, List<string>>();
        private string _text;

        public MainViewModel()
        {
            PropertyChanged += ValidateOnPropertyChanged;
        }

        private void ValidateOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Validate(e.PropertyName);
            ErrorsChanged?.Invoke(sender, new DataErrorsChangedEventArgs(e.PropertyName));
        }

        [Required(ErrorMessage = "This field is required")]
        [CustomValidation(typeof(MainViewModel), nameof(ValidationMethod))]
        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                OnPropertyChanged();
            }
        }

        public static ValidationResult ValidationMethod(object value, ValidationContext validationContext)
        {
            return ValidationResult.Success;
        }

        public bool HasErrors => _validationResults.Any(x => x.Value?.Any() == true);

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public IEnumerable GetErrors(string propertyName)
        {
            if (_validationResults.TryGetValue(propertyName, out var result))
            {
                return result;
            }

            return Enumerable.Empty<string>();
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Validate(string propertyName)
        {
            var property = GetType().GetProperty(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException();
            }

            var validationAttributes = property.GetCustomAttributes<ValidationAttribute>();
            var results = new List<string>();

            foreach(var validationAttribute in validationAttributes)
            {
                var value = property.GetValue(this);
                var validationContext = new ValidationContext(value);
                var result = validationAttribute.GetValidationResult(value, validationContext);
                if (result != null)
                {
                    results.Add(result.ErrorMessage);
                }
            }

            _validationResults[propertyName] = results;
        }
    }
}
