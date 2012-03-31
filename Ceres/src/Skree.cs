using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ivy
{
    public class Skree : Entity
    {
        //private AnimGraph m_animGraph;

        private Texture2D m_spriteMap;

        public Skree(Game game) :
            base(game)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            m_spriteMap = Game.Content.Load<Texture2D>("Sprites\\skree");

            m_animGraph = new AnimGraph(this);
            m_animGraph.Initialize();

            Rectangle rotateAnimRect = new Rectangle(0, 0, 88, 30);
            AnimatedSprite rotateAnim = new AnimatedSprite(m_spriteMap, rotateAnimRect, 4, 10);
            rotateAnim.Initialize();
            rotateAnim.Scale = new Vector2(3f, 3f);

            Rectangle attackRect = new Rectangle(88, 0, 26, 26);
            AnimatedSprite attackAnim = new AnimatedSprite(m_spriteMap, attackRect, 1, 1);
            attackAnim.Initialize();
            attackAnim.Scale = new Vector2(3f, 3f);

            IAnimGraphNode rotateNode = m_animGraph.AddAnim(rotateAnim);
            IAnimGraphNode attackNode = m_animGraph.AddAnim(attackAnim);

            m_animGraph.AddTransition(rotateNode, MessageType.ActivateSkree, attackNode);
            m_animGraph.AddTransition(attackNode, MessageType.ActivateSkree, rotateNode);

            m_animGraph.SetCurrentNode(rotateNode);

        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            m_animGraph.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            m_animGraph.Draw(spriteBatch);
        }

        public override void ReceiveMessage(Message msg)
        {
            base.ReceiveMessage(msg);

            m_animGraph.ReceiveMessage(msg);
        }
    }
}
