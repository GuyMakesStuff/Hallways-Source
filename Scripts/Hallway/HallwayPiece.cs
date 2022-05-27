using System.Collections;
using System.Collections.Generic;
using Experiments.EndlessHallway.Managers;
using UnityEngine;

namespace Experiments.EndlessHallway.Gameplay.Generation
{
    public class HallwayPiece : MonoBehaviour
    {
        [Header("Walls")]
        public GameObject FrontWall;
        public GameObject BackWall;
        public GameObject LeftWall;
        public GameObject RightWall;
        [HideInInspector]
        public bool IsEnd;
        [HideInInspector]
        public bool SplitsLeft;
        [HideInInspector]
        public bool SplitsRight;
        [HideInInspector]
        public bool IsSplit;
        [Header("Other")]
        public HallwayManager.HallwayBlock RefBlock;
        public GameObject TokenPrefab;
        bool SpawnedToken;

        public void Construct(HallwayManager.HallwayBlock Block)
        {
            IsEnd = Block.DeadEnd;
            SplitsLeft = Block.SplitToLeft;
            SplitsRight = Block.SplitToRight;

            RefBlock = Block;
        }

        // Update is called once per frame
        void Update()
        {
            if(RefBlock.SplitToLeft && Vector3.Distance(transform.position, HallwayManager.Instance.CamPos) < HallwayManager.Instance.CamRange) { RefBlock.ConstructLeftHallway(); }
            if(RefBlock.SplitToRight && Vector3.Distance(transform.position, HallwayManager.Instance.CamPos) < HallwayManager.Instance.CamRange) { RefBlock.ConstructRightHallway(); }

            FrontWall.SetActive(IsEnd);
            BackWall.SetActive(false);
            LeftWall.SetActive(!SplitsLeft);
            RightWall.SetActive(!SplitsRight);
        }

        public void TrySpawnToken()
        {
            if(IsEnd && !IsSplit)
            {
                float RandPer = Random.value * 100f;
                Instantiate(TokenPrefab, transform.position - Vector3.up, Quaternion.identity, transform).GetComponent<TokenContainer>().SpawnValue = RandPer;
            }
        }
    }
}