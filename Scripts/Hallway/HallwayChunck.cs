using System.Collections;
using System.Collections.Generic;
using Experiments.EndlessHallway.Managers;
using UnityEngine;

namespace Experiments.EndlessHallway.Gameplay.Generation
{
    public class HallwayChunck : MonoBehaviour
    {
        public int Length;
        List<HallwayManager.HallwayBlock> Blocks;
        List<HallwayManager.HallwayBlock> SplittableBlocks;

        public void Construct(List<HallwayManager.HallwayBlock> Pieces)
        {
            Blocks = new List<HallwayManager.HallwayBlock>();
            for (int P = 0; P < Length; P++)
            {
                Vector3 SpawnPos = HallwayManager.Instance.CoordToWorldPos(Pieces[P].Position);
                HallwayPiece hallwayPiece = Instantiate(HallwayManager.Instance.HallwayPiecePrefab, SpawnPos, transform.rotation, transform).GetComponent<HallwayPiece>();
                hallwayPiece.Construct(Pieces[P]);
                Pieces[P].OBJ = hallwayPiece;
                Blocks.Add(Pieces[P]);
            }

            CreateSplits();
            TrySpawningTokens();
        }

        void TrySpawningTokens()
        {
            foreach (HallwayManager.HallwayBlock B in Blocks)
            {
                B.OBJ.TrySpawnToken();
                HallwayManager.Instance.ReadyPercent = HallwayManager.Instance.ReadyCountdown;
            }
        }

        void CreateSplits()
        {
            int SplitCount = Random.Range(HallwayManager.Instance.MinSplits, HallwayManager.Instance.MaxSplits + 1);
            SplittableBlocks = new List<HallwayManager.HallwayBlock>(Blocks);
            CheckForBlockSplitMode(0);
            CheckForBlockSplitMode(SplittableBlocks.Count - 1);
            for (int S = 0; S < SplitCount; S++)
            {
                int BlockIndex = Random.Range(0, SplittableBlocks.Count);
                SplitBlock(BlockIndex);
            }
        }
        void CheckForBlockSplitMode(int BlockIndex)
        {
            HallwayManager.SplitMode splitMode = HallwayManager.SplitMode.Never;
            if(BlockIndex == 0) { splitMode = HallwayManager.Instance.SplitFirstBlock; }
            else if(BlockIndex == SplittableBlocks.Count - 1) { splitMode = HallwayManager.Instance.SplitLastBlock; }
            
            switch (splitMode)
            {
                case HallwayManager.SplitMode.Must:
                {
                    SplitBlock(BlockIndex);
                    break;
                }
                case HallwayManager.SplitMode.Optional:
                {
                    break;
                }
                case HallwayManager.SplitMode.Never:
                {
                    SplittableBlocks.RemoveAt(BlockIndex);
                    break;
                }
            }
        }
        void SplitBlock(int BlockIndex)
        {
            if(SplittableBlocks.Count == 0)
            {
                return;
            }

            SplittableBlocks[BlockIndex].SplitToLeft = Random.value > 0.5f;
            SplittableBlocks[BlockIndex].SplitToRight = Random.value > 0.5f;
            SplittableBlocks[BlockIndex].Reconstruct();
            SplittableBlocks.RemoveAt(BlockIndex);
            HallwayManager.Instance.ReadyPercent = HallwayManager.Instance.ReadyCountdown;
        }
    }
}
