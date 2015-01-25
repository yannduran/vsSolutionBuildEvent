﻿/*
 * The MIT License (MIT)
 * 
 * Copyright (c) 2013-2015  Denis Kuzmin (reg) <entry.reg@gmail.com>
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
*/

using System;
using System.Runtime.InteropServices;

namespace net.r_eg.vsSBE.Provider
{
    [Guid("536FFC0D-8BC1-4347-B4B4-694308F9E396")]
    public interface ILibrary
    {
        /// <summary>
        /// Absolute path to used library
        /// </summary>
        string Dllpath { get; }

        /// <summary>
        /// Name of used library with full path
        /// </summary>
        string FullName { get; }

        /// <summary>
        /// Version of used library
        /// </summary>
        Bridge.IVersion Version { get; }

        /// <summary>
        /// All public events of used library
        /// </summary>
        Bridge.IEvent Event { get; }

        /// <summary>
        /// The Build operations of used library
        /// </summary>
        Bridge.IBuild Build { get; }

        /// <summary>
        /// Settings of used library
        /// </summary>
        Bridge.ISettings Settings { get; }
    }
}
