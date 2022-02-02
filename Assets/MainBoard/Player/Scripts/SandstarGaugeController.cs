using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JSF.Game.Player
{
    public class SandstarGaugeController : MonoBehaviour
    {
        public Player Player;
        public GameObject SandstarGaugePrefab;
        public SandstarGaugeCell[] Cells;
        // Start is called before the first frame update
        void Start()
        {
            Cells = new SandstarGaugeCell[GlobalVariable.MaxSandstar];
            for(var i = 0; i < GlobalVariable.MaxSandstar; i++)
            {
                GameObject CellObj = Instantiate(SandstarGaugePrefab);
                CellObj.transform.SetParent(transform, false);
                Cells[i] = CellObj.GetComponent<SandstarGaugeCell>();
                Cells[i].SetStatus(SandstarGaugeStatus.Inactive);
            }
        }

        // Update is called once per frame
        void Update()
        {
            for (var i = 0; i < GlobalVariable.MaxSandstar; i++)
            {
                Cells[i].SetStatus(Player.SandstarAmount>i ? SandstarGaugeStatus.Active : SandstarGaugeStatus.Inactive);
            }
        }

        public void PlayGaugeAnimation(SandstarGaugeStatus status, int amount, float time)
        {
            for (var i = 0; i < amount; i++)
            {
                Cells[i].SetStatus(status, time);
            }
        }
    }

}