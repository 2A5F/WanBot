﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WanBot.Api.Event;
using WanBot.Api.Util;

namespace WanBot.Api.Mirai.Event
{
    /// <summary>
    /// 事件基类
    /// </summary>
    [JsonConverter(typeof(PolymorphicJsonConverter<BaseMiraiEvent>))]
    public class BaseMiraiEvent : BlockableEventArgs, ISerializablePolymorphic
    {
        public string Type => GetType().Name;
    }
}
