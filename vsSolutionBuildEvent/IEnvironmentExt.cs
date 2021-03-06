﻿/*
 * Copyright (c) 2013-2015  Denis Kuzmin (reg) <entry.reg@gmail.com>
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using Microsoft.VisualStudio.Shell.Interop;

namespace net.r_eg.vsSBE
{
    /// <summary>
    /// Extended variant of environment
    /// </summary>
    public interface IEnvironmentExt
    {
        /// <summary>
        /// Gets project name from IVsHierarchy
        /// </summary>
        /// <param name="pHierProj"></param>
        /// <param name="force">Load in global collection with __VSHPROPID.VSHPROPID_ExtObject if true value</param>
        /// <returns></returns>
        string getProjectNameFrom(IVsHierarchy pHierProj, bool force);
    }
}
