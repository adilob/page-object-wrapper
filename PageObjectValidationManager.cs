using PageObjectWrapper.AutomationUI.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageObjectWrapper.AutomationUI
{
    public class PageObjectValidationManager
    {
        private const string VALIDATION_IDENTIFIER = "ValidationIdentifier";
        private static PageObjectValidationManager _singleton = null;
        public static PageObjectValidationManager Instance
        {
            get
            {
                if (_singleton == null)
                    _singleton = new PageObjectValidationManager();

                return _singleton;
            }
        }

        private IList<Type> Validators;
        private IDictionary<Type, object> _internalInstances;
        private IDictionary<Tuple<string, string, string, string, string, string>, object> _internalInstancesByTuple;

        private PageObjectValidationManager()
        {
            InitializeValidators();
            _internalInstances = new Dictionary<Type, object>();
            _internalInstancesByTuple = new Dictionary<Tuple<string, string, string, string, string, string>, object>();
        }

        private void InitializeValidators()
        {
            var allTypes = GetType().Assembly.GetTypes();
            var validationTypes = allTypes; //.Where(t => Attribute.IsDefined(t, typeof(ValidationObjectConditionAttribute)));

            Validators = validationTypes.ToList();
        }

        public void AddValidator(Type type)
        {
            Validators.Add(type);
        }

        public PageObjectValidationBase<T> GetValidator<T>(string validationIdentifier, PageObjectBase<T> pageObject)
            where T : IAutomationPageObject
        {
            foreach (var type in Validators)
            {
                var staticProp = type.GetProperty(VALIDATION_IDENTIFIER, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                var id = staticProp.GetValue(type, null);

                if (id.Equals(validationIdentifier))
                {
                    if (_internalInstances.ContainsKey(type))
                        return _internalInstances[type] as PageObjectValidationBase<T>;
                    else
                    {
                        _internalInstances.Add(type, Activator.CreateInstance(type, pageObject));
                        return _internalInstances[type] as PageObjectValidationBase<T>;
                    }
                }
            }

            return null;
        }

        //public PageObjectValidationBase<T> GetValidatorByCondition<T>(PageObjectBase<T> pageObject, string activity, string country, string product, string type, string subtype, string option)
        //    where T : IAutomationPageObject
        //{
        //    var tupleKey = new Tuple<string, string, string, string, string, string>(activity, country, product, type, subtype, option);
        //    if (_internalInstancesByTuple.ContainsKey(tupleKey))
        //    {
        //        return _internalInstancesByTuple[tupleKey] as PageObjectValidationBase<T>;
        //    }

        //    foreach (var validatorType in Validators)
        //    {
        //        var attribs = validatorType.GetCustomAttributes(typeof(ValidationObjectConditionAttribute), false);
        //        if (attribs == null || attribs.Length <= 0)
        //            continue;

        //        foreach (var item in attribs)
        //        {
        //            var attrib = item as ValidationObjectConditionAttribute;
        //            if (attrib != null)
        //            {
        //                var isActivityValid = attrib.Activity.Equals(activity);
        //                if (!isActivityValid) continue;

        //                var isCountryValid = !string.IsNullOrEmpty(attrib.Country) ? attrib.Country.Equals(country) : true; // optional
        //                if (!isCountryValid) continue;

        //                var isProductValid = !string.IsNullOrEmpty(attrib.Product) ? attrib.Product.Equals(product) : false; // required
        //                if (!isProductValid) continue;

        //                var isTypeValid = !string.IsNullOrEmpty(attrib.Type) ? attrib.Type.Equals(type) : false;
        //                if (!isTypeValid) continue;

        //                var isSubTypeValid = !string.IsNullOrEmpty(attrib.SubType) ? (!subtype.Equals("None") && !subtype.Equals("--None--")) ? attrib.SubType.Equals(subtype) : true : true; // optional
        //                if (!isSubTypeValid) continue;

        //                var isOptionValid = !string.IsNullOrEmpty(attrib.Option) ? (!option.Equals("None") && !option.Equals("--None--")) ? attrib.Option.Equals(option) : true : true; // optional
        //                if (!isOptionValid) continue;

        //                if (!_internalInstances.ContainsKey(validatorType))
        //                    _internalInstances.Add(validatorType, Activator.CreateInstance(validatorType, pageObject));

        //                if (!_internalInstancesByTuple.ContainsKey(tupleKey))
        //                    _internalInstancesByTuple.Add(tupleKey, _internalInstances[validatorType]);

        //                return _internalInstances[validatorType] as PageObjectValidationBase<T>;
        //            }
        //        }
        //    }

        //    return null;
        //}

        public PageObjectValidationBase<T> GetValidatorByType<U, T>(PageObjectBase<T> pageObject)
            where U : PageObjectValidationBase<T>
            where T : IAutomationPageObject
        {
            foreach (var type in Validators)
            {
                //if (type.Equals(typeof(U)))
                if (typeof(U).Equals(type))
                {
                    if (_internalInstances.ContainsKey(type))
                        return _internalInstances[type] as PageObjectValidationBase<T>;
                    else
                    {
                        _internalInstances.Add(type, Activator.CreateInstance(type, pageObject));
                        return _internalInstances[type] as PageObjectValidationBase<T>;
                    }
                }
            }

            return null;
        }
    }
}
