using System.Collections.Generic;
using UnityEngine;

public class PromptObjPairs : MonoBehaviour
{
    public static Dictionary<string, PromptObjs> promptObjPairs = new Dictionary<string, PromptObjs>()
    {
        // // surfaces - airport
        // // 3 obj
        //{ "flat rectangular security checkpoint table used at airport screening areas", new PromptObjs(new List<string> { "as1_605491615", "as1_623450287", "as1_1737627896_good" }, 0) },
        //// 3 obj
        //{ "simple metal table used for inspecting personal travel items at airports", new PromptObjs(new List<string> { "as2_788299368", "as2_1404375662_good", "as2_904920359_good" }, 0) },

        //// airport
        //// 1  - out of place
        //{ "coiled phone charger cable with plug", new PromptObjs(new List<string> { "charger_856934901_good" }, 0) },
        //// 2
        //{ "paperback book for reading while flying", new PromptObjs(new List<string> { "book_307306434_good", "book_934383058_good" }, 0) },
        //// 1
        //{ "small charging case for wireless earbuds", new PromptObjs(new List<string> { "earbuds_678385212_good" }, 0) },
        //// 1 - out of place
        //{ "compact travel camera with a zoom lens and a wrist strap", new PromptObjs(new List<string> { "camera_1704829580_good" }, 0) },       
        //// 2
        //{ "leather wallet used to store money and cards", new PromptObjs(new List<string> { "wallet_1992885337_good", "wallet_721488479_good" }, 0) },
        //// 2 - out of place
        //{ "firm zippered case designed to protect sunglasses during air travel", new PromptObjs(new List<string> { "sunglasses_945576735_good", "sunglasses_1576693440_good" }, 0) },


        // surfaces - retail
        // 2
        //{ "flat wooden grocery store table used for showcasing products", new PromptObjs(new List<string> { "gs1_590818909_good", "gs1_1086683929_good" }, 0) },
        ////3 
        //{ "metal retail display table found in grocery stores", new PromptObjs(new List<string> { "gs2_1174808971_good", "gs2_684773294_good", "gs2_459824098_good" }, 0) },
        //// retail
        //// 1
        //{ "plastic jar with lid, filled with creamy peanut butter", new PromptObjs(new List<string> { "peanut_1325380111_good" }, 0) },
        ////1 
        //{ "colorful cardboard box of breakfast cereal", new PromptObjs(new List<string> { "cereal_1903903851_good" }, 0) },
        //// 1
        //{ "rectangular carton of fresh milk with a screw cap", new PromptObjs(new List<string> { "milk_1618120201_good" }, 0) },
        //// 1
        //{ "clear plastic grocery bag filled with uncooked spaghetti noodles", new PromptObjs(new List<string> { "spaghetti_333562915_good" }, 0) },
        //// 1
        //{ "small white plastic cup of yogurt with a foil lid", new PromptObjs(new List<string> { "yogurt_1887312003_good" }, 0) },
        //// 1
        //{ "solid rectangular block of yellow cheese used for slicing or cooking", new PromptObjs(new List<string> { "cheese_1244943072_good" }, 0) },

        
        // surfaces - factory
        // 2
        //{ "flat metal work table used for sorting and organizing factory items", new PromptObjs(new List<string> { "fs1_1072545744_good", "fs1_1703564787_good" }, 0) },
        //// 4
        //{ "sturdy industrial workbench used for assembling tools and parts in a factory", new PromptObjs(new List<string> { "fs2_196797231", "fs2_332196284", "fs2_481296246", "fs2_902459339_good" }, 0) },
        //// factory
        //// 2
        //{ "handheld toolbox with a top handle and multiple compartments inside", new PromptObjs(new List<string> { "handheld_toolbox_2007814054", "handheld_toolbox_1531983384_good" }, 0) },
        //// 1
        //{ "paper label storage box commonly used for organizing printed factory labels", new PromptObjs(new List<string> { "label_box_344361041_good" }, 0) },
        //// 2
        //{ "set of adjustable metal wrench used for tightening bolts and nuts", new PromptObjs(new List<string> { "wrench_1395326832", "wrench_342573711_good" }, 0) },
        //// 1
        //{ "set of hex keys in plastic holder", new PromptObjs(new List<string> { "hex_keys_1612589_good" }, 0) },
        //// 2
        //{ "retractable measuring tape inside a rectangular plastic housing", new PromptObjs(new List<string> { "measuring_tape_1422770149_good", "measuring_tape_1525161585_good" }, 0) },
        //// 1
        //{ "small plastic box used for storing factory screws and tiny fasteners", new PromptObjs(new List<string> { "plastic_box_323040741_good" }, 0) },

        // RPG environment
        { "dry ground with patches of grass", new PromptObjs(new List<string> { "ground_69666904_good" }, 0) },
        { "green low height bushes", new PromptObjs(new List<string> { "bushes_144995132", "bushes_1558584881_good" }, 0) },
        { "small wooden hut made of wood logs", new PromptObjs(new List<string> { "hut_384712971_good" }, 0) },
        { "chest with gold coins", new PromptObjs(new List<string> { "chest_227062069_good" }, 0) },
        //{ "pink flower", new PromptObjs(new List<string> { "flower_124742751_good" }, 0) },
        { "set grey rocks", new PromptObjs(new List<string> { "rocks_508405739", "rocks_275186169_good" }, 0) },
        { "greenish leafy tree", new PromptObjs(new List<string> { "tree_733047917_good", "tree_855861459_good", "tree_1275887521_good" }, 0) }
    };


    public static string FindObjByPrompt(string prompt)
    {
        if (!promptObjPairs.ContainsKey(prompt))
        {
            Debug.LogError("Prompt not found: " + prompt);
            return null;
        }

        PromptObjs promptObj = promptObjPairs[prompt];
        if (promptObj.ObjectIds == null || promptObj.ObjectIds.Count == 0)
        {
            Debug.LogError("No object IDs found for prompt: " + prompt);
            return null;
        }

        if (promptObj.Status == promptObj.ObjectIds.Count) promptObj.Status = 0;
        string objectID = promptObj.ObjectIds[promptObj.Status++];

        return objectID;
    }
}
