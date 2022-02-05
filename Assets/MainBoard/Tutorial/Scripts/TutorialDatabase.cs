using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JSF.Game.Tutorial
{
    public class TutorialDatabase
    {
        public static Tutorial[] tutorials = new Tutorial[]
        {
            // チュートリアル1 移動と狩り
            new Tutorial()
            {
                InitialBoardStatus = new BoardStatus()
                {
                    Size=new Vector2Int(3,3),
                    Cells=new BoardCell[]
                    {
                        new BoardCell()
                        {
                            Pos = new Vector2Int(1,0),
                            Dir = RotationDirection.FORWARD,
                            FriendName = "Serval",
                            PlayerID = 0
                        },
                        new BoardCell()
                        {
                            Pos = new Vector2Int(1,2),
                            Dir = RotationDirection.BACKWARD,
                            FriendName = "Dhole",
                            PlayerID = 1,
                        },
                        new BoardCell()
                        {
                            Pos = new Vector2Int(0,0),
                            Dir = RotationDirection.FORWARD,
                            FriendName = "Serval",
                            PlayerID = 0
                        },
                        new BoardCell()
                        {
                            Pos = new Vector2Int(2,2),
                            Dir = RotationDirection.BACKWARD,
                            FriendName = "Dhole",
                            PlayerID = 1
                        },
                    },
                },
                InitialSandstarAmount = 0,
                Nodes = new TutorialNode[]
                {
                    new TutorialNode(){
                        Text="ようこそお祭りへ！私は案内役の[TODO]です、よろしくお願いします！\n\n\n<size=70%>クリックorタップで次に進みます。</size>"
                    },
                    new TutorialNode(){
                        Text="このお祭りは、フレンズ数人が2チームに分かれて狩りごっこで戦う団体戦です！"
                    },
                    new TutorialNode(){
                        FocusedGameObjectName="GameBoard"
                    },
                    new TutorialNode(){
                        Text="それぞれのチームにはリーダーがいます！リーダーはこのように濃い枠で囲まれています！",
                        FocusedCell=new Vector2Int[]{new Vector2Int(1,0) },
                        TextPosAnchor=new Vector2(0.3f,0.8f)
                    },
                    new TutorialNode(){
                        Text="相手チームのリーダーを狩ると勝利です！",
                        FocusedCell=new Vector2Int[]{new Vector2Int(1,2) },
                        TextPosAnchor=new Vector2(0.3f,0.2f)
                    },
                    new TutorialNode()
                    {
                        Text="このゲームでは交互にターンが回ってきます！"
                    },
                    new TutorialNode
                    {
                        Text="1ターンの間に、フレンズを1人動かすことができます！試しにこのフレンズを動かしてみましょう！",
                        FocusedCell=new Vector2Int[]{new Vector2Int(0,0) },
                        TextPosAnchor=new Vector2(0.3f,0.8f)
                    },
                    new TutorialNode
                    {
                        Text="動かすには、そのフレンズに触れてから行き先のマスに触れて下さい！\n\n<color=#808080>左下のフレンズを1つ上のマスに動かしてみましょう。</color>",
                        FocusedCell=new Vector2Int[] { new Vector2Int(0, 0), new Vector2Int(0,1) },
                        TextPosAnchor=new Vector2(0.3f,0.8f),
                        MovableFriendsId = new int[]{0},
                        MovablePos = new Vector2Int[]{new Vector2Int(0,1)},
                        TutorialCondition = (GameManager, TutorialManager) =>
                        {
                            if(GameManager.Map.TryGetValue(new Vector2Int(0, 1), out Board.Cell cell))
                            {
                                if (cell.Friends)
                                {
                                    if(cell.Friends == TutorialManager.GetFriendById(2))
                                    {
                                        return true;
                                    }
                                }
                            }
                            return false;
                        }
                    },
                    new TutorialNode
                    {
                        Text="よくできました！\n\n次は相手のターンです、相手の出方を観察してみましょう。",
                        TextPosAnchor=new Vector2(0.3f,0.8f),
                    },
                    new TutorialNode
                    {
                        Text="",
                        TextPosAnchor=new Vector2(0.3f,0.8f),
                        TutorialCondition = (GameManager, TutorialManager) =>
                        {
                            return true;
                        },
                        AutoMoveStrategy = (GameManager, TutorialManager) =>
                        {
                            return GameManager.MoveFriendWithAnimation(
                                GameManager.Map[new Vector2Int(1,2)].Friends,
                                GameManager.Map[new Vector2Int(0,1)],
                                false
                            );
                        },
                    },
                    new TutorialNode
                    {
                        Text="あっ！仲間が狩られてしまいました…でもこれで相手にスキができました！",
                        TextPosAnchor=new Vector2(0.3f,0.8f),
                    },
                    new TutorialNode
                    {
                        Text="このフレンズが相手のリーダーを狩れそうです！斜めに動いて狩ってみましょう！",

                        FocusedCell=new Vector2Int[] { new Vector2Int(1, 0), new Vector2Int(0,1) },
                        TextPosAnchor=new Vector2(0.3f,0.8f),
                        MovableFriendsId = new int[]{2},
                        MovablePos = new Vector2Int[]{new Vector2Int(0,1)},
                        TutorialCondition = (GameManager, TutorialManager) =>
                        {
                            if(GameManager.Map.TryGetValue(new Vector2Int(0, 1), out Board.Cell cell))
                            {
                                if (cell.Friends)
                                {
                                    if(cell.Friends == TutorialManager.GetFriendById(0))
                                    {
                                        return true;
                                    }
                                }
                            }
                            return false;
                        }
                    },
                    new TutorialNode
                    {
                        Text="お見事です！これで相手のリーダーが狩れたので、私たちの勝利です！"
                    },
                    new TutorialNode
                    {
                        Text="これで「移動」「狩る」の操作の説明は以上です。\n\n<color=#808080>クリックorタップでタイトル画面に戻ります。</color>"
                    }
                }
            },

            
            // チュートリアル2 回転と特殊スキル
            new Tutorial()
            {
                InitialBoardStatus = new BoardStatus()
                {
                    Size=new Vector2Int(4,3),
                    Cells=new BoardCell[]
                    {
                        new BoardCell()
                        {
                            Pos = new Vector2Int(1,0),
                            Dir = RotationDirection.FORWARD,
                            FriendName = "Serval",
                            PlayerID = 0
                        },
                        new BoardCell()
                        {
                            Pos = new Vector2Int(3,2),
                            Dir = RotationDirection.BACKWARD,
                            FriendName = "Dhole",
                            PlayerID = 1,
                        },
                        new BoardCell()
                        {
                            Pos = new Vector2Int(0,0),
                            Dir = RotationDirection.FORWARD_RIGHT,
                            FriendName = "Serval",
                            PlayerID = 0
                        },
                        new BoardCell()
                        {
                            Pos = new Vector2Int(2,2),
                            Dir = RotationDirection.BACKWARD,
                            FriendName = "Dhole",
                            PlayerID = 1
                        },
                    },
                },
                InitialSandstarAmount = 3,
                Nodes = new TutorialNode[]
                {
                    new TutorialNode(){
                        Text="あっ来た来た！今日はとっておきの技を教えるよ！\n\n\n<size=70%>クリックorタップで次に進みます。</size>"
                    },
                    new TutorialNode(){
                        Text="このゲーム、普通の「将棋」と違って、駒の向きが変えられるんだ！"
                    },
                    new TutorialNode(){
                        Text="例えばこの子、少し右に傾いているでしょ？",
                        FocusedCell=new Vector2Int[]{new Vector2Int(0,0) },
                        TextPosAnchor=new Vector2(0.8f,0.8f)
                    },
                    new TutorialNode(){
                        Text="この向きによって、駒の動ける場所が変わるんだ！\n試しにこの子を回転させてみよー！",
                        FocusedCell=new Vector2Int[]{new Vector2Int(1,0) },
                        TextPosAnchor=new Vector2(0.3f,0.2f)
                    },
                    new TutorialNode
                    {
                        Text="回転させるには、そのフレンズに<b>2回</b>触れてから\n向きたいほうにあるマスに触れてね！\n\n<color=#808080>リーダーのフレンズを右上向きにしてみましょう。</color>",
                        FocusedCell=new Vector2Int[] { new Vector2Int(1, 0), new Vector2Int(2,1) },
                        TextPosAnchor=new Vector2(0.3f,0.8f),
                        MovableFriendsId = new int[]{0},
                        MovablePos = new Vector2Int[]{new Vector2Int(1,0)},
                        TutorialCondition = (GameManager, TutorialManager) =>
                        {
                            if(GameManager.Map.TryGetValue(new Vector2Int(1, 0), out Board.Cell cell))
                            {
                                if (cell.Friends)
                                {
                                    if(cell.Friends == TutorialManager.GetFriendById(0))
                                    {
                                        if(cell.Friends.Dir == RotationDirection.FORWARD_RIGHT)
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                            return false;
                        }
                    },
                    new TutorialNode
                    {
                        Text="よくできました！はなまる！",
                        TextPosAnchor=new Vector2(0.3f,0.2f),
                    },
                    new TutorialNode
                    {
                        Text="次は相手のターンだよ！相手のSandstarゲージに注目しててね！",
                        TextPosAnchor=new Vector2(0.3f,0.2f),
                        FocusedGameObjectName = "Game/ViewCanvas/ScaleSupporter/Background/GameBoard/Players/Player2/SandstarViewer"
                    },
                    new TutorialNode
                    {
                        Text="",
                        TextPosAnchor=new Vector2(0.3f,0.8f),
                        TutorialCondition = (GameManager, TutorialManager) =>
                        {
                            return true;
                        },
                        AutoMoveStrategy = (GameManager, TutorialManager) =>
                        {
                            return GameManager.MoveFriend(
                                GameManager.Map[new Vector2Int(3,2)].Friends,
                                new Vector2Int(3,2),
                                RotationDirection.BACKWARD_LEFT,
                                false
                            );
                        },
                    },
                    new TutorialNode
                    {
                        Text="いま相手も回転したんだけど、サンドスターが1つ増えたよね！？",
                        TextPosAnchor=new Vector2(0.3f,0.2f),
                    },
                    new TutorialNode
                    {
                        Text="サンドスターは1ターンに1つ増えるよ！\nあと、何も動かさずにサンドスターを追加で1つもらうこともできるよ！",
                        TextPosAnchor=new Vector2(0.3f,0.2f),
                    },
                    new TutorialNode
                    {
                        Text="このサンドスターを使うと特殊スキルを使うことができるよ！\n特殊スキルは、それぞれのフレンズの得意なことなんだ！",
                        TextPosAnchor=new Vector2(0.3f,0.2f),
                    },
                    new TutorialNode
                    {
                        Text="例えば、サーバルちゃんは大きくジャンプすることが得意だから、目の前のフレンズを飛び越えて行くことができるよ！",
                        FocusedCell = new Vector2Int[]{ new Vector2Int(1,0) },
                        TextPosAnchor=new Vector2(0.3f,0.8f),
                    },
                    new TutorialNode
                    {
                        Text="特殊スキルを使うには、そのフレンズを3回触って、黄色のマスが出たらそこを触ってみて！",

                        FocusedCell=new Vector2Int[] { new Vector2Int(1, 0), new Vector2Int(3,2) },
                        TextPosAnchor=new Vector2(0.3f,0.8f),
                        MovableFriendsId = new int[]{0},
                        MovablePos = new Vector2Int[]{new Vector2Int(3,2)},
                        TutorialCondition = (GameManager, TutorialManager) =>
                        {
                            if(GameManager.Map.TryGetValue(new Vector2Int(3, 2), out Board.Cell cell))
                            {
                                if (cell.Friends)
                                {
                                    if(cell.Friends == TutorialManager.GetFriendById(0))
                                    {
                                        return true;
                                    }
                                }
                            }
                            return false;
                        }
                    },
                    new TutorialNode
                    {
                        Text="相手めがけて大ジャンプ！！\n特殊スキルの効果はフレンズによって全然違うから、使う前にスキル情報を確認するといいよ！"
                    },
                    new TutorialNode
                    {
                        Text="「回転」「特殊スキル」について分かったかな？\n分からないことがあればまた何回でも聞きに来てね！\n\n<color=#808080>クリックorタップでタイトル画面に戻ります。</color>"
                    }
                }
            }


        };
    }

    public struct Tutorial
    {
        public BoardStatus InitialBoardStatus;
        public TutorialNode[] Nodes;
        public int InitialSandstarAmount;
    }

    public struct BoardStatus
    {
        public Vector2Int Size;
        public BoardCell[] Cells;
    }
    public struct BoardCell
    {
        public Vector2Int Pos;
        public RotationDirection Dir;
        public string FriendName;
        public int PlayerID;
    }

    public struct TutorialNode
    {
        public string Text;
        public Vector2? TextPosAnchor;
        public Vector2Int[] FocusedCell;
        public string FocusedGameObjectName;
        public int[] MovableFriendsId;
        public Vector2Int[] MovablePos;
        public TutorialCondition TutorialCondition;
        public AutoMoveStrategy AutoMoveStrategy;
    }

    public delegate bool TutorialCondition(GameManager Manager, TutorialManager TutorialManager);
    public delegate IEnumerator AutoMoveStrategy(GameManager Manager, TutorialManager TutorialManager);
}