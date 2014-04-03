﻿/* 
 * Boost Software License - Version 1.0 - August 17th, 2003
 * 
 * Copyright (c) 2013 Developed by reg <entry.reg@gmail.com>
 * 
 * Permission is hereby granted, free of charge, to any person or organization
 * obtaining a copy of the software and accompanying documentation covered by
 * this license (the "Software") to use, reproduce, display, distribute,
 * execute, and transmit the Software, and to prepare derivative works of the
 * Software, and to permit third-parties to whom the Software is furnished to
 * do so, all subject to the following:
 * 
 * The copyright notices in the Software and this entire statement, including
 * the above license grant, this restriction and the following disclaimer,
 * must be included in all copies of the Software, in whole or in part, and
 * all derivative works of the Software, unless such copies or derivative
 * works are solely in the form of machine-executable object code generated by
 * a source language processor.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE, TITLE AND NON-INFRINGEMENT. IN NO EVENT
 * SHALL THE COPYRIGHT HOLDERS OR ANYONE DISTRIBUTING THE SOFTWARE BE LIABLE
 * FOR ANY DAMAGES OR OTHER LIABILITY, WHETHER IN CONTRACT, TORT OR OTHERWISE,
 * ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE. 
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace net.r_eg.vsSBE
{
    internal class Config
    {
        /// <summary>
        /// SBE data at runtime
        /// </summary>
        public static SolutionEvents Data
        {
            get { return data; }
        }
        protected static SolutionEvents data = null;

        protected struct Entity
        {
            /// <summary>
            /// Current config version
            /// Notice: version of app is controlled by Package
            /// </summary>
            public const string VERSION = "0.4";

            /// <summary>
            /// into file system
            /// </summary>
            public const string NAME    = ".vssbe";
        }

        /// <summary>
        /// Current location
        /// </summary>
        public static string WorkPath
        {
            get { return _path; }
        }
        private static string _path = "";

        /// <summary>
        /// identification with full path
        /// </summary>
        private static string _Link
        {
            get { return _path + Entity.NAME; }
        }

        /// <summary>
        /// Initialization settings
        /// </summary>
        /// <param name="path">path to configuration file</param>
        public static void load(string path)
        {
            _path = path;
            _xprojvsbeUpgrade();

            data = new SolutionEvents();
            try
            {
                using(FileStream stream = new FileStream(_Link, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    XmlSerializer xml   = new XmlSerializer(typeof(SolutionEvents));
                    data                = (SolutionEvents)xml.Deserialize(stream);
                    compatibilityCheck(stream);
                }
                Log.nlog.Info("loaded settings: {0}\n\nReady:", _path);
            }
            catch(FileNotFoundException)
            {
                Log.nlog.Info("Initialize with new settings");
            }
            catch(Exception e)
            {
                Log.nlog.Fatal("Configuration file is corrupt {0}", e.Message);
                //TODO: choice actions /UI
            }

            // now compatibility should be updated to the latest
            data.settings.compatibility = Entity.VERSION;
        }

        /// <summary>
        /// with changing path
        /// </summary>
        /// <param name="path">path to configuration file</param>
        public static void save(string path)
        {
            _path = path;
            save();
        }

        public static void save()
        {
            using(TextWriter stream = new StreamWriter(_Link))
            {
                if(data == null){
                    data = new SolutionEvents();
                }
                XmlSerializer xml = new XmlSerializer(typeof(SolutionEvents));
                xml.Serialize(stream, data);
            }
        }

        /// <summary>
        /// Older versions support :: Check version and reorganize structure if needed..
        /// </summary>
        /// <param name="stream"></param>
        protected static void compatibilityCheck(FileStream stream)
        {
            Version ver = Version.Parse(data.settings.compatibility);

            if(ver.Major == 0 && ver.Minor < 4)
            {
                Log.show();
                Log.nlog.Info("Start upgrade configuration 0.3 -> 0.4");
                Upgrade.Migration03_04.migrate(stream);
                //TODO: to ErrorList
                Log.nlog.Warn("Successfully upgraded. *Please, save manually!");
            }
        }

        /// <summary>
        /// Older versions support :: Change name settings
        /// </summary>
        /// <returns></returns>
        private static void _xprojvsbeUpgrade()
        {
            string oldcfg = _path + ".xprojvsbe";
            if(!(File.Exists(oldcfg) && !File.Exists(_Link))) {
                return;
            }

            try {
                File.Move(oldcfg, _Link);
                Log.nlog.Info("Successfully upgraded settings :: .xprojvsbe -> {0}", Entity.NAME);
            }
            catch(Exception e) {
                Log.nlog.Fatal("Failed upgrade .xprojvsbe\n\n-----\n{0}\n", e.Message);
            }
        }


        protected Config(){}
    }
}
