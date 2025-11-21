using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;


namespace Tdd20.TestAdapter
{
    public static class PropertyRegistrar
    {
        public static readonly TestProperty MyStringProperty = TestProperty.Register(
            id: "Native C++ Test Name, Filename and Line Number",
            label: "Native C++ Test Name, Filename and Line Number",
            valueType: typeof(string),
            owner: typeof(TestCase));
    }
}
