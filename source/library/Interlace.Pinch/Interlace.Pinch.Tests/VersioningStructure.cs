using System;
using System.Collections.Generic;
using System.Text;

namespace Interlace.Pinch.TestsVersion3
{
    public partial class VersioningStructure
    {
        void OnMissingNewFields(int decodedUpToVersion, int decodingVersion)
        {
            if (decodingVersion == 2)
            {
                _addedOptPointer = "Added1";
                _addedOptScalar = 2;
                _addedOptStructure = new SmallStructure();
                _addedOptStructure.Test = 3;
                _addedReqPointer = "Added4";
                _addedReqScalar = 5;
                _addedReqStructure = new SmallStructure();
                _addedReqStructure.Test = 6;
            }
        }
    }
}
