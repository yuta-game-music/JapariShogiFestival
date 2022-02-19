using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSF.Common;
using JSF.Game;
using JSF.Game.Player;
using System.Linq;
using System;
using UnityEngine.Serialization;
using JSF.Game.Logger;

namespace JSF.Database
{

    [CreateAssetMenu(fileName = "Author.asset", menuName = "JSF/Friends/AuthorData")]
    public class AuthorData : ScriptableObject
    {
        public string Name;
        public Sprite Image;
        
    }
}