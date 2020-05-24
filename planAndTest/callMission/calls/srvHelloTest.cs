﻿using commonLib;
using models.calls;
using System;
using System.Collections.Generic;
using System.Text;

namespace callMission.calls
{
    public class srvHelloTest : srvBase
    {
        /// <summary>
        /// the do call procedure
        /// </summary>
        /// <param name="inputJson">call type in json</param>
        /// <returns>return type in json</returns>
        public override string doCall(string inputJson)
        {
            string outputJson;
            clsHelloTest inOut;
            inOut = jsonUtl.decodeJson<clsHelloTest>
                ( inputJson);
            inOut.returnPara = string.Format(
                @"Hello, {0}", inOut.callPara);
            outputJson = jsonUtl.encodeJson(inOut);
            return outputJson;
        }

    }
}