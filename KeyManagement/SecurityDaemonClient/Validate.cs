// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// -----------------------------------------------------------------------

using System;

namespace Microsoft.Azure.EventGridEdge.IotEdge
{
    internal static class Validate
    {
        public static void ArgumentNotNull(object value, string paramName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(paramName, $"The argument {paramName} is null.");
            }
        }

        public static void ArgumentNotNullOrEmpty(string value, string paramName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(paramName, $"The argument {paramName} is null.");
            }
            else if (value.Length == 0)
            {
                throw new ArgumentException(paramName, $"The argument {paramName} is empty.");
            }
        }
    }
}
