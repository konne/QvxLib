/*
    This Library is to have an easy access to Qvx Files and the Qlikview
    Connector Interface.
  
    Copyright (C) 2011  Konrad Mattheis (mattheis@ukma.de)
 
    This Software is available under the GPL and a comercial licence.
    For further information to the comercial licence please contact
    Konrad Mattheis. 

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
*/

namespace QvxLib
{
    #region Usings
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;
    #endregion

    #region QVXWindow
    public sealed class QvxWindow : IWin32Window
    {
        #region Constructors
        public QvxWindow(int handle)
        {
            Handle = new IntPtr(handle);
        }

        public QvxWindow(IntPtr handle)
        {
            Handle = handle;
        } 
        #endregion

        #region IWin32Window
        public IntPtr Handle { get; private set; } 
        #endregion
    }
    #endregion
}
