﻿using System.Collections.Generic;

namespace ConanExilesHelper.Models.Configuration;

public class ConanExilesSettings
{
    public List<ulong>? ChannelIdFilter { get; set; } = new List<ulong>();
    public ConanExilesServerEntry Server { get; set; } = new ConanExilesServerEntry();
}
