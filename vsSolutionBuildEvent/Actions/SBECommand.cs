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
using System.Diagnostics;
using EnvDTE80;
using System.Runtime.InteropServices;
using System.Globalization;
using net.r_eg.vsSBE.Events;
using net.r_eg.vsSBE.Exceptions;

namespace net.r_eg.vsSBE.Actions
{
    public class SBECommand
    {
        const string CMD_DEFAULT = "cmd";

        public class ShellContext
        {
            public string path;
            public string disk;

            public ShellContext(string path)
            {
                this.path = path;
                this.disk = getDisk(path);
            }

            protected string getDisk(string path)
            {
                if(String.IsNullOrEmpty(path)) {
                    throw new SBEException("path is empty or null");
                }
                return path.Substring(0, 1);
            }
        }

        /// <summary>
        /// The current OEM code page from the system locale
        /// </summary>
        protected int OEMCodePage
        {
            get
            {
                CultureInfo inf = CultureInfo.GetCultureInfo(GetSystemDefaultLCID());
                return inf.TextInfo.OEMCodePage;
            }
        }

        protected MSBuildParser parser;

        /// <summary>
        /// Used environment
        /// </summary>
        protected Environment env;

        /// <summary>
        /// Special raw data from the output window pane
        /// </summary>
        protected string owpDataRaw;

        /// <summary>
        /// Current working context for scripts or files
        /// </summary>
        protected ShellContext context;

        /// <summary>
        /// basic implementation
        /// </summary>
        /// <param name="evt">provided sbe-events</param>
        public bool basic(ISolutionEvent evt)
        {
            if(!evt.enabled){
                return false;
            }

            string cfg = env.SolutionConfigurationFormat(env.SolutionActiveConfiguration);

            if(evt.toConfiguration != null 
                && evt.toConfiguration.Length > 0 && evt.toConfiguration.Where(s => s == cfg).Count() < 1)
            {
                Log.nlog.Info("Action '{0}' is ignored for current configuration - '{1}'", evt.caption, cfg);
                return false;
            }

            Log.nlog.Info("Launching action '{0}' :: Configuration - '{1}'", evt.caption, cfg);
            switch(evt.mode) {
                case TModeCommands.Operation: {
                    Log.nlog.Info("Use Operation Mode");
                    return hModeOperation(evt);
                }
                case TModeCommands.Interpreter: {
                    Log.nlog.Info("Use Interpreter Mode");
                    return hModeScript(evt);
                }
            }
            Log.nlog.Info("Use File Mode");
            return hModeFile(evt);
        }

        /// <summary>
        /// Addition accompanying information from assembly
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="raw"></param>
        /// <returns></returns>
        public bool supportOWP(ISolutionEvent evt, string raw)
        {
            owpDataRaw = raw;
            return basic(evt);
        }

        public SBECommand(Environment env, MSBuildParser parser)
        {
            this.env    = env;
            this.parser = parser;
        }

        public void updateContext(ShellContext context)
        {
            this.context = context;
        }

        protected virtual bool hModeFile(ISolutionEvent evt)
        {
            string cFiles = evt.command;

            parseVariables(evt, ref cFiles);
            useShell(evt, _treatNewlineAs(" & ", cFiles));

            return true;
        }

        protected virtual bool hModeOperation(ISolutionEvent evt)
        {
            if(evt.dteExec.cmd == null || evt.dteExec.cmd.Length < 1) {
                return true;
            }
            (new DTEOperation((EnvDTE.DTE)env.DTE2)).exec(evt.dteExec.cmd, evt.dteExec.abortOnFirstError);
            return true;
        }

        protected virtual bool hModeScript(ISolutionEvent evt)
        {
            if(evt.interpreter.Trim().Length < 1){
                Log.nlog.Warn("interpreter not selected");
                return false;
            }
            string script = evt.command;

            parseVariables(evt, ref script);
            script = _treatNewlineAs(evt.newline, script);

            switch(evt.wrapper.Length) {
                case 1: {
                    script = string.Format("{0}{1}{0}", evt.wrapper, script.Replace(evt.wrapper, "\\" + evt.wrapper));
                    break;
                }
                case 2: {
                    //pair as: (), {}, [] ...
                    //e.g.: (echo str&echo.&echo str) >> out
                    string wL = evt.wrapper.ElementAt(0).ToString();
                    string wR = evt.wrapper.ElementAt(1).ToString();
                    script = string.Format("{0}{1}{2}", wL, script.Replace(wL, "\\" + wL).Replace(wR, "\\" + wR), wR);
                    break;
                }
            }

            useShell(evt, string.Format("{0} {1}", evt.interpreter, script));
            return true;
        }

        [DllImport("kernel32.dll")]
        protected static extern int GetSystemDefaultLCID();

        protected void useShell(ISolutionEvent evt, string cmd)
        {
            ProcessStartInfo psi = new ProcessStartInfo(CMD_DEFAULT);
            if(evt.processHide) {
                psi.WindowStyle = ProcessWindowStyle.Hidden;
            }
            //psi.StandardErrorEncoding = psi.StandardOutputEncoding = Encoding.GetEncoding(OEMCodePage);

            string args = String.Format("/C cd {0}{1} & {2}",
                                        context.path,
                                        (context.disk != null) ? " & " + context.disk + ":" : "", cmd);

            if(!evt.processHide && evt.processKeep) {
                args += " & pause";
            }

            Log.nlog.Info(cmd);

            //TODO: stdout/stderr capture & add to OWP

            psi.Arguments       = args;
            Process process     = new Process();
            process.StartInfo   = psi;
            process.Start();

            if(evt.waitForExit) {
                process.WaitForExit();
            }
        }

        protected void parseVariables(ISolutionEvent evt, ref string data)
        {
            data = parser.parseVariablesSBE(data, SBECustomVariable.OWP_BUILD, owpDataRaw);
            data = parser.parseVariablesSBE(data, SBECustomVariable.OWP_BUILD_WARNINGS, null); // reserved
            data = parser.parseVariablesSBE(data, SBECustomVariable.OWP_BUILD_ERRORS, null);   // reserved

            if(evt.parseVariablesMSBuild) {
                data = parser.parseVariablesMSBuild(data);
            }
        }

        private string _treatNewlineAs(string str, string data)
        {
            return data.Trim(new char[]{'\r', '\n'}).Replace("\r", "").Replace("\n", str);
        }
    }
}
