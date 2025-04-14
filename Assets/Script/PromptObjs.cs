using System.Collections.Generic;
using UnityEngine;

public class PromptObjs
{
    public List<string> ObjectIds { get; set; }
    public int Status { get; set; }

    public PromptObjs(List<string> objectIds, int status)
    {
        ObjectIds = objectIds;
        Status = status;
    }
}