﻿//******************************************************************************
// <copyright file="license.md" company="RawCMS project  (https://github.com/arduosoft/RawCMS)">
// Copyright (c) 2019 RawCMS project  (https://github.com/arduosoft/RawCMS)
// RawCMS project is released under GPL3 terms, see LICENSE file on repository root at  https://github.com/arduosoft/RawCMS .
// </copyright>
// <author>Daniele Fontani, Emanuele Bucarelli, Francesco Min�</author>
// <autogenerated>true</autogenerated>
//******************************************************************************
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace RawCMS.Client.BLL.Core
{
    public class LogProvider
    {
        private static ServiceProvider _provider = null;

        public static Runner Runner
        {
            get
            {
                if (_provider == null)
                {
                    _provider = new ServiceCollection()
                         .AddLogging(builder =>
                         {
                             builder.SetMinimumLevel(LogLevel.Trace);
                             builder.AddNLog(new NLogProviderOptions
                             {
                                 CaptureMessageTemplates = true,
                                 CaptureMessageProperties = true
                             });
                         })
                         .AddTransient<Runner>()
                         .BuildServiceProvider();
                }

                return _provider.GetRequiredService<Runner>();
            }
        }
    }
}