using System;
using System.Collections.Generic;
using System.Text;
using Interlace.Binding;
using Interlace.Binding.AutoBinders;

namespace Interlace.Binding.AutoBinders
{
    public static class DevExpressBindingFactories
    {
        static bool _registered = false;

        public static void Register()
        {
            if (!_registered)
            {
                _registered = true;

                Binder.RegisterAutoBindingFactory(new BaseEditAutoBindingFactory());
                Binder.RegisterAutoBindingFactory(new LabelAutoBindingFactory());
            }
        }
    }
}
