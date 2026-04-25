using Godot;
using System;
using System.Data.Common;


[GlobalClass]
public partial class LevenDefinitionResource : Resource
{
    [Export]
    public string Id { get; private set; }

    [Export]
    public int StartingResourceCount { get; private set; } = 4;

    [Export(PropertyHint.File, "*.tscn")]
    public string levelScenePath { get; private set; }
}
