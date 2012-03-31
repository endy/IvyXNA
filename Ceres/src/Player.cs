using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;


namespace Ivy
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class Player : Entity
    {
        Weapon m_armCannon;
       
        // Sound Effects
        SoundEffect m_rollEffect;
        SoundEffectInstance m_rollInstance;
        SoundEffect m_landEffect;

        public Player(IvyGame game) : base(game)
        {

        }

        public override void Initialize()
        {
            Movable = true;
            Damagable = true;

            m_animGraph = new AnimGraph(this);
            m_animGraph.Initialize();
            
            m_armCannon = new Weapon(this, new Point(10, 10), Direction);
            m_armCannon.Initialize();

            m_landEffect = Game.Content.Load<SoundEffect>("Audio\\samus_land");
            m_rollEffect = Game.Content.Load<SoundEffect>("Audio\\samus_jump_roll");
            m_rollInstance = m_rollEffect.CreateInstance();

            #region Anim Rects
            Rectangle samusTurnRect = new Rectangle(0, 156, 78, 47);
            Rectangle samusWaitRightRect = new Rectangle(0, 203, 140, 45);
            Rectangle samusWaitLeftRect = new Rectangle(0, 248, 140, 45);
            Rectangle samusRunRightRect = new Rectangle(0, 66, 460, 45);
            Rectangle samusRunLeftRect = new Rectangle(0, 111, 460, 45);
            Rectangle samusJumpRollRightRect = new Rectangle(0, 0, 280, 32);
            Rectangle samusJumpRollLeftRect = new Rectangle(0, 33, 280, 32);
            Rectangle samusJumpAscendRightRect = new Rectangle(0, 293, 56, 47);
            Rectangle samusJumpAscendLeftRect = new Rectangle(0, 340, 56, 47);
            Rectangle samusJumpDescendRightRect = new Rectangle(56, 293, 112, 47);
            Rectangle samusJumpDescendLeftRect = new Rectangle(56, 340, 112, 47);
            Rectangle samusJumpLandRightRect = new Rectangle(168, 293, 56, 47);
            Rectangle samusJumpLandLeftRect = new Rectangle(168, 340, 56, 47);
            #endregion

            Texture2D samusMap = Game.Content.Load<Texture2D>("Sprites\\samusMap");
           
            #region Animation Setup
            AnimatedSprite samusTurnLeftAnim = new AnimatedSprite(samusMap, samusTurnRect, 3, 24f);

            IAnimGraphNode samusTurnLeftNode = m_animGraph.AddAnim(samusTurnLeftAnim);
            samusTurnLeftNode.Anim.Initialize();
            samusTurnLeftNode.Anim.Loop = false;
            samusTurnLeftNode.Anim.Name = "SamusTurnLeft";

            AnimatedSprite samusTurnRightAnim = new AnimatedSprite(samusMap, samusTurnRect, 3, 24f);

            IAnimGraphNode samusTurnRightNode = m_animGraph.AddAnim(samusTurnRightAnim);
            samusTurnRightNode.Anim.Initialize();
            samusTurnRightNode.Anim.Loop = false;
            samusTurnRightNode.Anim.Reverse = true;
            samusTurnRightNode.Anim.Name = "SamusTurnRight";           

            IAnimGraphNode samusWaitLeftNode = m_animGraph.AddAnim(
                new AnimatedSprite(samusMap, samusWaitLeftRect, 5, 6f));
            samusWaitLeftNode.Anim.Initialize();
            samusWaitLeftNode.Anim.Name = "SamusWaitLeft";

            IAnimGraphNode samusWaitRightNode = m_animGraph.AddAnim( 
                new AnimatedSprite(samusMap, samusWaitRightRect, 5, 6f));
            samusWaitRightNode.Anim.Initialize();
            samusWaitRightNode.Anim.Name = "SamusWaitRight";

            IAnimGraphNode samusRunLeftNode = m_animGraph.AddAnim(
                new AnimatedSprite(samusMap, samusRunLeftRect, 10, 18f));
            samusRunLeftNode.Anim.Initialize();
            samusRunLeftNode.Anim.Name = "SamusRunLeft";

            IAnimGraphNode samusRunRightNode = m_animGraph.AddAnim(
                new AnimatedSprite(samusMap, samusRunRightRect, 10, 18f));
            samusRunRightNode.Anim.Initialize();
            samusRunRightNode.Anim.Name = "SamusRunRight";

            // Stand & Run code
            m_animGraph.AddTransition(samusWaitLeftNode, MessageType.MoveLeft, samusRunLeftNode);
            m_animGraph.AddTransition(samusRunLeftNode, MessageType.Stand, samusWaitLeftNode);

            m_animGraph.AddTransition(samusWaitRightNode, MessageType.MoveRight, samusRunRightNode);
            m_animGraph.AddTransition(samusRunRightNode, MessageType.Stand, samusWaitRightNode);

            // Turn Code

            // Turning Left to Right
            m_animGraph.AddTransition(samusWaitLeftNode, MessageType.MoveRight, samusTurnRightNode);
            m_animGraph.AddTransition(samusRunLeftNode, MessageType.MoveRight, samusTurnRightNode);
            m_animGraph.AddTransitionOnAnimEnd(samusTurnRightNode, samusRunRightNode);
            m_animGraph.AddTransitionOnAnimEnd(samusTurnRightNode, MessageType.Stand, samusWaitRightNode);
            m_animGraph.AddTransitionOnAnimEnd(samusTurnRightNode, MessageType.MoveRight, samusRunRightNode);
            
            // Turning Right to Left            
            m_animGraph.AddTransition(samusWaitRightNode, MessageType.MoveLeft, samusTurnLeftNode);
            m_animGraph.AddTransition(samusRunRightNode, MessageType.MoveLeft, samusTurnLeftNode);
            m_animGraph.AddTransitionOnAnimEnd(samusTurnLeftNode, samusRunLeftNode);
            m_animGraph.AddTransitionOnAnimEnd(samusTurnLeftNode, MessageType.Stand, samusWaitLeftNode);
            m_animGraph.AddTransitionOnAnimEnd(samusTurnLeftNode, MessageType.MoveLeft, samusRunLeftNode);
            
            // Change turn direction           
            m_animGraph.AddTransition(samusTurnLeftNode, MessageType.MoveRight, samusTurnRightNode);
            m_animGraph.AddTransition(samusTurnRightNode, MessageType.MoveLeft, samusTurnLeftNode);


            IAnimGraphNode samusJumpRollLeftNode = m_animGraph.AddAnim(
                new AnimatedSprite(samusMap, samusJumpRollLeftRect, 8, 16f));
            samusJumpRollLeftNode.Anim.Initialize();

            IAnimGraphNode samusJumpRollRightNode = m_animGraph.AddAnim(
                new AnimatedSprite(samusMap, samusJumpRollRightRect, 8, 16f));
            samusJumpRollRightNode.Anim.Initialize();

            IAnimGraphNode samusJumpAscendLeftNode = m_animGraph.AddAnim(
                new AnimatedSprite(samusMap, samusJumpAscendLeftRect, 2, 16f));
            samusJumpAscendLeftNode.Anim.Initialize();
            samusJumpAscendLeftNode.Anim.Loop = false;

            IAnimGraphNode samusJumpAscendRightNode = m_animGraph.AddAnim(
                new AnimatedSprite(samusMap, samusJumpAscendRightRect, 2, 16f));
            samusJumpAscendRightNode.Anim.Initialize();
            samusJumpAscendRightNode.Anim.Loop = false;

            IAnimGraphNode samusJumpDescendLeftNode = m_animGraph.AddAnim(
                new AnimatedSprite(samusMap, samusJumpDescendLeftRect, 4, 16f));
            samusJumpDescendLeftNode.Anim.Initialize();
            samusJumpDescendLeftNode.Anim.Loop = false;

            IAnimGraphNode samusJumpDescendRightNode = m_animGraph.AddAnim(
                new AnimatedSprite(samusMap, samusJumpDescendRightRect, 4, 16f));
            samusJumpDescendRightNode.Anim.Initialize();
            samusJumpDescendRightNode.Anim.Loop = false;

            AnimatedSprite landLeftAnim = new AnimatedSprite(samusMap, samusJumpLandLeftRect, 2, 16f);

            IAnimGraphNode samusJumpRollLandLeftNode = m_animGraph.AddAnim(landLeftAnim);
            samusJumpRollLandLeftNode.Anim.Initialize();
            samusJumpRollLandLeftNode.Anim.Loop = false;

            IAnimGraphNode samusJumpDescendLandLeftNode = m_animGraph.AddAnim(landLeftAnim);
            samusJumpDescendLandLeftNode.Anim.Initialize();
            samusJumpDescendLandLeftNode.Anim.Loop = false;

            AnimatedSprite landRightAnim = new AnimatedSprite(samusMap, samusJumpLandRightRect, 2, 16f);

            IAnimGraphNode samusJumpRollLandRightNode = m_animGraph.AddAnim(landRightAnim);
            samusJumpRollLandRightNode.Anim.Initialize();
            samusJumpRollLandRightNode.Anim.Loop = false;

            IAnimGraphNode samusJumpDescendLandRightNode = m_animGraph.AddAnim(landRightAnim);
            samusJumpDescendLandRightNode.Anim.Initialize();
            samusJumpDescendLandRightNode.Anim.Loop = false;

            // Jump Ascend & Descend
            m_animGraph.AddTransition(samusRunLeftNode, MessageType.Jump, samusJumpRollLeftNode);
            m_animGraph.AddTransition(samusRunRightNode, MessageType.Jump, samusJumpRollRightNode);

            m_animGraph.AddTransition(samusWaitLeftNode, MessageType.Jump, samusJumpAscendLeftNode);
            m_animGraph.AddTransition(samusWaitRightNode, MessageType.Jump, samusJumpAscendRightNode);

            m_animGraph.AddTransition(samusJumpAscendLeftNode, MessageType.Fall, samusJumpDescendLeftNode);
            m_animGraph.AddTransition(samusJumpAscendRightNode, MessageType.Fall, samusJumpDescendRightNode);

            // Jump Turn
            m_animGraph.AddTransition(samusJumpRollLeftNode, MessageType.MoveRight, samusJumpRollRightNode);
            m_animGraph.AddTransition(samusJumpRollRightNode, MessageType.MoveLeft, samusJumpRollLeftNode);
            m_animGraph.AddTransition(samusJumpAscendLeftNode, MessageType.MoveRight, samusJumpAscendRightNode);
            m_animGraph.AddTransition(samusJumpAscendRightNode, MessageType.MoveLeft, samusJumpAscendLeftNode);
            m_animGraph.AddTransition(samusJumpDescendLeftNode, MessageType.MoveRight, samusJumpDescendRightNode);
            m_animGraph.AddTransition(samusJumpDescendRightNode, MessageType.MoveLeft, samusJumpDescendLeftNode);

            // Land
            m_animGraph.AddTransition(samusJumpRollLeftNode, MessageType.Land, samusJumpRollLandLeftNode);
            m_animGraph.AddTransitionOnAnimEnd(samusJumpRollLandLeftNode, samusRunLeftNode);
            m_animGraph.AddTransitionOnAnimEnd(samusJumpRollLandLeftNode, MessageType.Stand, samusWaitLeftNode);
            m_animGraph.AddTransitionOnAnimEnd(samusJumpRollLandLeftNode, MessageType.MoveLeft, samusRunLeftNode);
            m_animGraph.AddTransitionOnAnimEnd(samusJumpRollLandLeftNode, MessageType.MoveRight, samusTurnRightNode);


            m_animGraph.AddTransition(samusJumpDescendLeftNode, MessageType.Land, samusJumpDescendLandLeftNode);
            m_animGraph.AddTransitionOnAnimEnd(samusJumpDescendLandLeftNode, samusWaitLeftNode);


            m_animGraph.AddTransition(samusJumpRollRightNode, MessageType.Land, samusJumpRollLandRightNode);
            m_animGraph.AddTransitionOnAnimEnd(samusJumpRollLandRightNode, samusRunRightNode);

            m_animGraph.AddTransition(samusJumpDescendRightNode, MessageType.Land, samusJumpDescendLandRightNode);
            m_animGraph.AddTransitionOnAnimEnd(samusJumpDescendLandRightNode, samusWaitRightNode);

            m_animGraph.SetCurrentNode(samusWaitRightNode);
            #endregion

            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            m_animGraph.Update(gameTime);
            m_armCannon.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            m_armCannon.Draw(spriteBatch);
            m_animGraph.Draw(spriteBatch);

            Console.WriteLine(Position);
        }

        public override void ReceiveMessage(Message msg)
        {
            switch (msg.Type)
            {
                case MessageType.FireWeapon:
                    FireWeapon();
                    break;
                default:
                    base.ReceiveMessage(msg);
                    break;
            }

            m_animGraph.ReceiveMessage(msg);

            if (msg.Type == MessageType.TakeDamage)
            {
                if (Energy <= 0)
                {
                    MessageDispatcher.Get().SendMessage(new Message(MessageType.EndGame, this, IvyGame.Get()));
                }
            }
        }
       
        private void FireWeapon()
        {
            m_armCannon.Fire();
        }        
    }
}