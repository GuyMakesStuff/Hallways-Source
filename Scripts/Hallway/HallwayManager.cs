using System.Collections;
using System.Collections.Generic;
using Experiments.Global.Managers;
using Experiments.EndlessHallway.Gameplay.Generation;
using UnityEngine;

namespace Experiments.EndlessHallway.Managers
{
    // This Script Generates An Endless Random Hallway.
    public class HallwayManager : Manager<HallwayManager>
    {
        // Conveniance Class For A Hallway Block MonoBehaviour (Or Hallway Piece)
        [System.Serializable]
        public class HallwayBlock
        {
            public Vector2Int Position;
            public bool SplitToLeft;
            bool GeneratedLeft;
            public bool SplitToRight;
            bool GeneratedRight;
            public bool DeadEnd;
            public HallwayPiece OBJ;

            // Reassign Properties For The Associated HallwayPiece MonoBehaviour
            public void Reconstruct()
            {
                OBJ.IsEnd = DeadEnd;
                OBJ.SplitsLeft = SplitToLeft;
                OBJ.SplitsRight = SplitToRight;
                OBJ.IsSplit = SplitToLeft || SplitToRight; // Used To Spawn Tokens
                OBJ.RefBlock = this;
            }

            // Construct A New Hallway With A Random Length To The Left
            public void ConstructLeftHallway()
            {
                // Only Generate If We Have Not Generated Anything Yet
                if(!GeneratedLeft)
                {
                    GeneratedLeft = true;
                    // Choose A Random Length For The Hallway
                    int HallwayLength = Random.Range(HallwayManager.Instance.MinLength, HallwayManager.Instance.MaxLength);
                    // Check If There Are Any Blocks In The Way.
                    // Also Use The CoordCast Function To Get The Distance Of 
                    // Blocks That Are In The Way To The Current Block.
                    int FinalHallwayLength = 0;
                    HallwayBlock NeighboringBlock = new HallwayBlock(); // (Conveniance) The HallwayBlock That The CoordCast Hits
                    bool CantConstruct = HallwayManager.Instance.CoordCast(Position, -OBJ.transform.right, HallwayLength, out FinalHallwayLength, ref NeighboringBlock);
                    // If There Is A Block Right Nearby, Cancel The Split
                    if(FinalHallwayLength == 1){ SplitToLeft = false; }
                    // If Not, Build A New Hallway To The Left
                    else
                    {
                        // The Length That The CoordCast Function Outputs In Inclusive,
                        // So If The Function Returns True (There Are Other Blocks In The Way),
                        // Decrease The Hallway Length By 1
                        if(CantConstruct) { FinalHallwayLength -= 1; }
                        // New Hallway Chunck Object Rotation
                        Quaternion Rot = Quaternion.LookRotation(-OBJ.transform.right, Vector3.up);
                        // New Hallway Chunck Object Position.
                        // The Positions Of The Blocks Are Inclusive To Their Associated Chunck, 
                        // So We Need To Offset That.
                        Vector2Int Pos = Position - HallwayManager.Instance.WorldPosToCoord(OBJ.transform.right);
                        // Build A New Hallway Chunck In The Given Length, Position And Rotation.
                        HallwayManager.Instance.NewHallway(Pos, FinalHallwayLength, Rot);
                    }
                    // Reconstruct The Block Object.
                    Reconstruct();
                }
            }
            // Construct A New Hallway With A Random Length To The Left.
            // Its Basically The Same Thing As ConstructLeftHallway But To The Right.
            public void ConstructRightHallway()
            {
                if(!GeneratedRight)
                {
                    GeneratedRight = true;
                    int HallwayLength = Random.Range(HallwayManager.Instance.MinLength, HallwayManager.Instance.MaxLength);
                    int FinalHallwayLength = 0;
                    HallwayBlock NeighboringBlock = new HallwayBlock(); 
                    bool CantConstruct = HallwayManager.Instance.CoordCast(Position, OBJ.transform.right, HallwayLength, out FinalHallwayLength, ref NeighboringBlock);
                    if(FinalHallwayLength == 1) { SplitToRight = false; }
                    else
                    {
                        Quaternion Rot = Quaternion.LookRotation(OBJ.transform.right, Vector3.up);
                        Vector2Int Pos = Position + HallwayManager.Instance.WorldPosToCoord(OBJ.transform.right);
                        HallwayManager.Instance.NewHallway(Pos, FinalHallwayLength - 1, Rot);
                    }
                    Reconstruct();
                }
            }
        }

        // Conveniance Class For A Singular Hallway MonoBehavoiur (Or Hallway Chunck)
        [System.Serializable]
        public class SingleHallway
        {
            public Vector2Int StartPosition;
            public int Length;
            public List<HallwayBlock> Pieces;
            public HallwayChunck OBJ;

            // Create The Blocks Of The Hallway Chunck (Class Form, No GameObjects)
            public void CreatePieces()
            {
                // Initialize A List Of Blocks
                Pieces = new List<HallwayBlock>(Length);
                // Create An Amount Of Blocks Given By The Length
                for (int P = 0; P < Length; P++)
                {
                    // Block Position
                    Vector3 OBJForward = OBJ.transform.forward;
                    Vector2Int Coord = StartPosition + HallwayManager.Instance.WorldPosToCoord(OBJForward) * P;
                    // Create New Hallway Block Class With A Given Position
                    // And Add It To The Blocks List
                    HallwayBlock NewHallwayBlock = new HallwayBlock()
                    {
                        Position = Coord,
                        DeadEnd = P == Length - 1, // If The Block Is In The End Of The Hallway, Check On The DeadEnd Boolean Of The New Block.
                    };
                    Pieces.Add(NewHallwayBlock);
                }
            }
        }

        [Header("General")]
        public int GameSeed;
        public bool GenerateOnStart = true;
        public GameObject HallwayPiecePrefab;
        public Transform ChuncksContainer;
        [HideInInspector]
        public List<SingleHallway> Hallways;
        public int HallwayReadyFrameTime;
        [HideInInspector]
        public float ReadyCountdown;
        [HideInInspector]
        public float ReadyPercent;
        bool IsReady;

        [Header("Generation Settings")]
        public int StartHallwayLength;
        public int MinLength;
        public int MaxLength;
        public int MinSplits;
        public int MaxSplits;
        public enum SplitMode { Must, Optional, Never }
        public SplitMode SplitFirstBlock;
        public SplitMode SplitLastBlock;

        [Header("Camera")]
        public Transform Cam;
        public float CamRange;
        [HideInInspector]
        public Vector3 CamPos;

        // Start is called before the first frame update
        void Awake()
        {
            // Initialize Manager Script
            Init(this);

            // Initialize Hallways List
            Hallways = new List<SingleHallway>();
            // Only Generate If GenerateOnStart Is True.
            // This Is Useful If We Dont Want To Instantly Generate The Hallway
            // And Rather Generate It Through Some Sort Of Game Manager.
            if(GenerateOnStart) { Generate(); }
        }

        // Generate The Random Hallway
        public void Generate()
        {
            // Reset The Ready Timer
            ReadyCountdown = HallwayReadyFrameTime * Time.deltaTime;
            ReadyPercent = ReadyCountdown;
            IsReady = false;
            // If A Hallway Is Already Generated,
            // Destroy All Already Generated Hallways.
            if(Hallways.Count > 0)
            {
                foreach (SingleHallway H in Hallways)
                {
                    Destroy(H.OBJ.gameObject);
                }
                Hallways.Clear();
            }
            // Initialize A Seed For Unitys Random Class
            Random.InitState(GameSeed);
            // Create The First Hallway.
            // Will Always Be At The Same Position (World Center), Length (StartHallwayLength), And Rotation (World Forward)
            NewHallway(Vector2Int.zero, StartHallwayLength, Quaternion.identity);
        }

        // Update is called once per frame
        void Update()
        {
            // Set The CamPos Vector To The Cameras Position
            CamPos = Cam.transform.position;

            // Update The Ready Countdown
            ReadyCountdown = HallwayReadyFrameTime * Time.deltaTime;
            ReadyPercent -= Time.deltaTime;
            IsReady = ReadyPercent <= float.Epsilon && GameManager.Instance.GameState != GameManager.GameStates.Init;

            // Only Set Hallway Blocks To Active If They Are In The Cameras Range (CamRange)
            foreach (SingleHallway SH in Hallways)
            {
                foreach (HallwayBlock B in SH.Pieces)
                {
                    B.OBJ.gameObject.SetActive(Vector3.Distance(CoordToWorldPos(B.Position), CamPos) < CamRange);
                }
            }
        }

        // A Function For Creating A New Hallway At A Given Position, Length And Rotation.
        public void NewHallway(Vector2Int startPosition, int Length, Quaternion Rot)
        {
            // Create Hallway Chunck (Class Form, No GameObjects)
            SingleHallway hallway = new SingleHallway()
            {
                StartPosition = startPosition,
                Length = Length
            };
            // Add The New Hallway Chunck To The Hallway Chuncks List
            Hallways.Add(hallway);
            // Construct The New Hallway Chunck
            ConstructHallway(hallway, Rot);
        }

        // Construct A Hallway Chunck GameObject With A HallwayChunck MonoBehaviour
        // With The Properties Of A Class From Hallway Chunck.
        void ConstructHallway(SingleHallway hallway, Quaternion Rotation)
        {
            // Create A New GameObject With A HallwayChunck MonoBehaviour In The Scene.
            HallwayChunck chunck = InstantiateEmptyObjectOfType<HallwayChunck>("Hallway Chunck");
            // Chunck Object Position (Converting The Hallway Classes Coordinates To World Coordinates)
            chunck.transform.position = CoordToWorldPos(hallway.StartPosition);
            // Chunck Object Rotation
            chunck.transform.rotation = Rotation;
            // Set The Chunck Objects HallwayChunck Components Length Variable.
            chunck.Length = hallway.Length;
            // Associate Hallway Class With Chunck Object
            hallway.OBJ = chunck;
            // Initialize Blocks List On Hallway Class
            hallway.CreatePieces();
            // Construct The Chunck Object
            chunck.Construct(hallway.Pieces);
        }

        // Convert Block Coordinates (Vector2Int) To World Coordinates (Vector3)
        public Vector3 CoordToWorldPos(Vector2Int Coord)
        {
            // A Single Hallway Block Takes Up 5 Units, So We Have To Multiply By 5
            return new Vector3(Coord.x * 5f, 0f, Coord.y * 5f);
        }
        // Convert World Coordinates (Vector3) To Block Coordinates (Vector2Int)
        public Vector2Int WorldPosToCoord(Vector3 Coord)
        {
            return new Vector2Int(Mathf.RoundToInt(Coord.x), Mathf.RoundToInt(Coord.z));
        }

        // Create An Empty GameObject In The Center Of The Scene
        // With A Specific Component To It And Return Set Component.
        public T InstantiateEmptyObjectOfType<T>(string Name) where T : Component
        {
            // Create New GameObject
            GameObject Result = new GameObject(Name);
            // Put The New GameObject In The Center Of The Scene.
            Result.transform.position = Vector3.zero;
            // Parent The New GameObject To The Chuncks Container.
            Result.transform.SetParent(ChuncksContainer);
            // Add A Component Of Type T To The GameObject And Return It (The Component).
            return Result.AddComponent<T>();
        }

        // Basically A Raycast Exept It Uses Block Coordinates And Instead Of Checking
        // For Collisions, It Checks For Hallway Blocks In The Way.
        public bool CoordCast(Vector2Int Coord, Vector3 Direction, int Length, out int FinalLength, ref HallwayBlock CastHit)
        {
            // Convert Direction To Block Coordinates
            Vector2Int NormalizedDirection = WorldPosToCoord(Direction);
            // We Create A For Loop That Begins In One And Runs Until It
            // Reaches *Length*, So Every Time The Loop Runs, The Index Of The Loop (C)
            // Will Be Closer And Closer To *Length*.
            for (int C = 1; C <= Length; C++)
            {
                // The Coordinate We Check If There Is A Block In.
                // It Is Equal To The Origin Coordinate Of The CoordCast (Coord),
                // Offseted By The Converted Direction Depending On The Loop Index (C).
                Vector2Int Pos = Coord + (NormalizedDirection * C);
                // The Block The CoordCast Hit (Conveniance)
                HallwayBlock HitBlock = new HallwayBlock();
                // If A Block Exists In *Pos (See Above)*, That Means The CoordCast Has Hit Something!
                if(CoordTaken(Pos, ref HitBlock))
                {
                    // Output The Block The CoordCast Hit (Conveniance)
                    CastHit = HitBlock;
                    // Output The Distance The CoordCast Has Traveled Before Hitting The Block (Inclusive)
                    FinalLength = C;
                    // Return True.
                    return true;
                }
            }

            // The CoordCast Has Not Hit Anything.
            // Output A Null Block Class
            CastHit = null;
            // Output The Distance The CoordCast Has Traveled (Since It Didn't Hit Anything, It Will Simply Be The Starting Length)
            FinalLength = Length;
            // Return False.
            return false;
        }

        // Check If A Hallway Block Already Exists In A Certain Coordinate.
        // Also, If There Is A Indeed A Block, Return It In Class Form (Conveniance).
        public bool CoordTaken(Vector2Int Coord, ref HallwayBlock block)
        {
            // Loops Over Every Generated Hallway
            foreach (SingleHallway SH in Hallways)
            {
                // Loops Over Every Block In The Hallway That Is Currently
                // Being Looped Over.
                foreach (HallwayBlock B in SH.Pieces)
                {
                    // If The Block That Is Currently Being Looped Overs Coordinate Is
                    // Equal To The Coord We Are Checking, Return True And Output The Block In Calss Form.
                    if(B.Position.x == Coord.x && B.Position.y == Coord.y) { block = B; return true; }
                }
            }

            // If A Block That Is In The Coord We Are Checking Is Not Found,
            // Return False And Output A Null Block Class.
            block = null;
            return false;
        }

        // Is The Hallway Fully Generated?
        public bool ReadyFunc()
        {
            return IsReady;
        }

        // Randomize Game Seed
        public void RandomizeSeed()
        {
            int NewGameSeed = new System.Random().Next();
            SettingsManager.Instance.FindInputFieldSetting("Game Seed").Value = NewGameSeed;
            UIManager.Instance.FixGameSeed(NewGameSeed);
            GameSeed = NewGameSeed;
            UIManager.Instance.PrevGameSeed = GameSeed;
        }
    }
}