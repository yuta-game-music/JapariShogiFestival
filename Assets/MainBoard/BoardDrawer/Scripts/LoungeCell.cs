using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using JSF.Database;
using JSF.Game.UI;

namespace JSF.Game.Board
{
    public class LoungeCell : Cell
    {

        protected Player.Player Possessor;
        protected RectTransform tf;
        // Start is called before the first frame update
        protected new void Start()
        {
            base.Start();
            
        }

        public void Setup(Player.Player Possessor, FriendOnBoard friend)
        {
            this.Possessor = Possessor;
            this.Friends = friend;
        }

        // Update is called once per frame
        protected new void Update()
        {
            base.Update();
        }

    }

}