using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ivy
{
    public interface IAnimGraphNode
    {
        AnimatedSprite Anim { get; }
    }

    public class AnimGraph : IMessageReceiver
    {
        #region AnimGraphNode class

        private class AnimGraphNode : IAnimGraphNode
        {
            public AnimatedSprite Anim { get; private set; }

            private AnimGraph m_graph;
            private Dictionary<MessageType, AnimGraphNode> m_nextNodesOnMessage;

            private AnimGraphNode m_nextNodeOnAnimEnd;
            private AnimGraphNode m_defaultNextNodeOnAnimEnd;
            private Dictionary<MessageType, AnimGraphNode> m_altNextNodesOnAnimEnd;


            public AnimGraphNode(AnimGraph graph, AnimatedSprite anim)
            {
                m_graph = graph;
                Anim = anim;

                Anim.OnAnimEnd += AnimatedSpriteEnd;

                m_nextNodesOnMessage = new Dictionary<MessageType, AnimGraphNode>();
                m_altNextNodesOnAnimEnd = new Dictionary<MessageType, AnimGraphNode>();
            }

            public virtual bool HandleMessage(Message msg)
            {
                bool handledMessage = true;
                if (m_nextNodesOnMessage.ContainsKey(msg.Type) == true)
                {
                    m_graph.SetCurrentNode(m_nextNodesOnMessage[msg.Type]);
                    m_nextNodeOnAnimEnd = m_defaultNextNodeOnAnimEnd;

                    //IvyGame.Get().ConsoleStr += " Handled Message: " + msg.Type + " in " + Anim.Name + "\n";
                }
                else if (m_altNextNodesOnAnimEnd.ContainsKey(msg.Type) == true)
                {
                    m_nextNodeOnAnimEnd = m_altNextNodesOnAnimEnd[msg.Type];

                    //IvyGame.Get().ConsoleStr += " Handled Message: " + msg.Type + " in " + Anim.Name + "\n";
                }
                else
                {
                    handledMessage = false;
                }

                return handledMessage;
            }

            public virtual bool AddTransition(MessageType msgType, AnimGraphNode nextNode)
            {
                bool added = false;
                if (m_nextNodesOnMessage.ContainsKey(msgType) == false)
                {
                    m_nextNodesOnMessage.Add(msgType, nextNode);
                    added = true;
                }

                return added;
            }
            public virtual bool AddTransitionOnAnimEnd(AnimGraphNode nextNode)
            {
                m_defaultNextNodeOnAnimEnd = nextNode;
                m_nextNodeOnAnimEnd = m_defaultNextNodeOnAnimEnd;

                return true;
            }

            public virtual bool AddTransitionOnAnimEnd(MessageType msgType, AnimGraphNode nextNode)
            {
                bool added = false;
                if (m_altNextNodesOnAnimEnd.ContainsKey(msgType) == false)
                {
                    m_altNextNodesOnAnimEnd.Add(msgType, nextNode);
                    added = true;
                }

                return added;
            }

            public void AnimatedSpriteEnd(AnimatedSprite anim)
            {
                if ((m_graph.GetCurrentNode() == this) && (m_nextNodeOnAnimEnd != null))
                {
                    m_graph.SetCurrentNode(m_nextNodeOnAnimEnd);
                    m_nextNodeOnAnimEnd = m_defaultNextNodeOnAnimEnd;
                }
            }
            
            // TODO: Add function to remove transitions
        }

        #endregion

        Entity m_owner;
        AnimGraphNode m_currentNode;
        List<AnimGraphNode> m_nodeList;

        public AnimGraph(Entity owner)
        {
            m_owner = owner;
        }

        public void Initialize()
        {
            m_nodeList = new List<AnimGraphNode>();
        }

        public void ReceiveMessage(Message msg)
        {
            if (m_currentNode != null)
            {
                m_currentNode.HandleMessage(msg);
            }
        }

        public void Update(GameTime gameTime)
        {
            if (m_currentNode != null)
            {
                m_currentNode.Anim.Update(gameTime);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (m_currentNode != null)
            {
                Vector2 roomPos = new Vector2(m_owner.Position.X, m_owner.Position.Y);
                Vector2 screenPos = IvyGame.Get().Camera.GetScreenPointForRoomPoint(roomPos);

                Point screenPosPoint = new Point((int)screenPos.X, (int)screenPos.Y);

                m_currentNode.Anim.Draw(spriteBatch, screenPosPoint);


                //IvyGame.Get().ConsoleStr += this + " Position (room): " + roomPos + "\n";
                //IvyGame.Get().ConsoleStr += this + " Position (screen): " + screenPos + "\n";
            }
        }

        public void Draw3D()
        {
            // does nothing currently
        }

        public IAnimGraphNode GetCurrentNode()
        {
            return m_currentNode;
        }

        public void SetCurrentNode(IAnimGraphNode iNode)
        {
            AnimGraphNode node = GetNode(iNode);

            // set node to current node
            if (node != null)
            {
                if (m_currentNode != null)
                {
                    //IvyGame.Get().ConsoleStr += "Change Node: " + m_currentNode.Anim.Name + " -> " + node.Anim.Name + "\n";
                    m_currentNode.Anim.Stop();

                    // a hack for syncing animations
                    // TODO: expose some way of turning this sync 'off'
                    //if (m_currentNode.Anim != node.Anim)
                    {
                        node.Anim.Reset();
                    }
                }                

                m_currentNode = node;
                m_currentNode.Anim.Play();
            }
            else
            {
                // TODO: error
            }
        }

        public IAnimGraphNode AddAnim(AnimatedSprite anim)
        {
            AnimGraphNode node = new AnimGraphNode(this, anim);

            m_nodeList.Add(node);

            return node;
        }

        public bool AddTransition(IAnimGraphNode iStartNode, MessageType msgType, IAnimGraphNode iEndNode)
        {
            AnimGraphNode startNode = GetNode(iStartNode);
            AnimGraphNode endNode = GetNode(iEndNode);

            bool added = false;

            if ((startNode != null) && (endNode != null))
            {
                startNode.AddTransition(msgType, endNode);
                added = true;
            }
            else
            {
                // TODO: error!
            }

            return added;
        }

        public bool AddTransitionOnAnimEnd(IAnimGraphNode startNode, IAnimGraphNode endNode)
        {
            AnimGraphNode internalStartNode = GetNode(startNode);
            AnimGraphNode internalEndNode = GetNode(endNode);

            bool added = false;

            if ((internalStartNode != null) && (internalEndNode != null))
            {
                if (internalStartNode.AddTransitionOnAnimEnd(internalEndNode) == true)
                {
                    added = true;
                }
            }
            return added;
        }

        public bool AddTransitionOnAnimEnd(IAnimGraphNode startNode, MessageType msgType, IAnimGraphNode endNode)
        {
            AnimGraphNode internalStartNode = GetNode(startNode);
            AnimGraphNode internalEndNode = GetNode(endNode);

            bool added = false;

            if ((internalStartNode != null) && (internalEndNode != null))
            {
                if (internalStartNode.AddTransitionOnAnimEnd(msgType, internalEndNode) == true)
                {
                    added = true;
                }
            }
            return added;
        }

        // TODO: why would i remove a node?  is there an anim graph editor?
        public void RemoveNode(IAnimGraphNode node)
        {
            // find node and remove it
            // find all references to node in other nodes, and remove it
        }

        // TODO: why would i remove a transition?  is there an anim graph editor?
        public void RemoveTransition(IAnimGraphNode start, MessageType msg, IAnimGraphNode end)
        {
            
        }

        public bool Contains(IAnimGraphNode node)
        {
            bool containsNode = false;
            foreach (AnimGraphNode n in m_nodeList)
            {
                if (n == node)
                {
                    containsNode = true;
                }
            }

            return containsNode;
        }

        private AnimGraphNode GetNode(IAnimGraphNode node)
        {
            foreach (AnimGraphNode n in m_nodeList)
            {
                if (n == node)
                {
                    return n;
                }
            }

            return null;
        }
    }
}
