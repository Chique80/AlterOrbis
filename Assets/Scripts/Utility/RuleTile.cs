using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Objects;
using SceneManagement;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace Utility
{
    [Serializable]
    [CreateAssetMenu]
    public class RuleTile : ChangingTile
    {
        [FormerlySerializedAs("m_DefaultSprite")]
        public Sprite mDefaultSprite;

        [FormerlySerializedAs("m_DefaultColliderType")]
        public ColliderType mDefaultColliderType;

        [Serializable]
        public class TilingRule
        {
            public Neighbor[] mNeighbors;
            public ChangingTile[] mSprites;
            public float mAnimationSpeed;
            public float mPerlinScale;
            public AutoTransform mAutoTransform;
            public OutputSprite mOutput;
            public ColliderType mColliderType;


            public TilingRule()
            {
                mOutput = OutputSprite.Single;
                mNeighbors = new Neighbor[8];
                mSprites = new ChangingTile[1];
                mAnimationSpeed = 1f;
                mPerlinScale = 0.5f;
                mColliderType = ColliderType.Sprite;

                for (var i = 0; i < mNeighbors.Length; i++)
                    mNeighbors[i] = Neighbor.DontCare;
            }

            public enum AutoTransform
            {
                Fixed,
                Rotated,
                MirrorX,
                MirrorY
            }

            public enum Neighbor
            {
                DontCare,
                This,
                NotThis
            }

            public enum OutputSprite
            {
                Single,
                Random,
                Animation
            }
        }

        [HideInInspector] public List<TilingRule> mTilingRules;

        public override void GetTileData(Vector3Int position, ITilemap tileMap, ref TileData tileData)
        {
            base.GetTileData(position, tileMap, ref tileData);
            tileData.colliderType = mDefaultColliderType;

            if (mTilingRules.Count > 1)
            {
                tileData.flags = TileFlags.LockTransform;
                tileData.transform = Matrix4x4.identity;
            }

            if (!GameObject.Find("AssetsRefresh")) return;
            if (!GameObject.Find("AssetsRefresh").GetComponent<AssetsRefresh>()) return;


            //tileData.sprite = AssetsRefresh.Instance.currentWorldType == AssetsRefresh.WorldType.Organic
            //   ? organic
            //   : mechanic;

            ApplyTilingRules(position, tileMap, ref tileData);
        }

        private void ApplyTilingRules(Vector3Int position, ITilemap tileMap, ref TileData tileData)
        {
            foreach (var rule in mTilingRules)
            {
                var matrix = Matrix4x4.identity;
                if (!RuleMatches(rule, position, tileMap, ref matrix)) continue;
                switch (rule.mOutput)
                {
                    case TilingRule.OutputSprite.Single:
                    case TilingRule.OutputSprite.Animation:
                        tileData.sprite = GameObject.Find("AssetsRefresh").GetComponent<AssetsRefresh>().currentWorldType == AssetsRefresh.WorldType.Organic
                            ? rule.mSprites[0].organic
                            : rule.mSprites[0].mechanic;
                        organic = rule.mSprites[0].organic;
                        mechanic = rule.mSprites[0].mechanic;
                        break;
                    case TilingRule.OutputSprite.Random:
                        var index = Mathf.Clamp(
                            Mathf.RoundToInt(Mathf.PerlinNoise((position.x + 1000000f) * rule.mPerlinScale,
                                (position.y + 1000000f) * rule.mPerlinScale) * rule.mSprites.Length), 0,
                            rule.mSprites.Length - 1);
                        tileData.sprite = GameObject.Find("AssetsRefresh").GetComponent<AssetsRefresh>().currentWorldType == AssetsRefresh.WorldType.Organic
                            ? rule.mSprites[index].organic
                            : rule.mSprites[index].mechanic;
                        organic = rule.mSprites[index].organic;
                        mechanic = rule.mSprites[index].mechanic;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                tileData.transform = matrix;
                //tileData.colliderType = rule.mColliderType;
                break;
            }
            
        }

        public override bool GetTileAnimationData(Vector3Int position, ITilemap tilemap,
            ref TileAnimationData tileAnimationData)
        {
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var rule in mTilingRules)
            {
                var matrix4 = Matrix4x4.identity;
                if (!RuleMatches(rule, position, tilemap, ref matrix4) ||
                    rule.mOutput != TilingRule.OutputSprite.Animation) continue;
                //tileAnimationData.animatedSprites = rule.m_Sprites;
                tileAnimationData.animationSpeed = rule.mAnimationSpeed;
                return true;
            }

            return false;
        }

        public override void RefreshTile(Vector3Int location, ITilemap tileMap)
        {
            if (mTilingRules != null && mTilingRules.Count > 0)
            {
                for (var y = -1; y <= 1; y++)
                {
                    for (var x = -1; x <= 1; x++)
                    {
                        base.RefreshTile(location + new Vector3Int(x, y, 0), tileMap);
                    }
                }
            }
            else
            {
                base.RefreshTile(location, tileMap);
            }
        }

        public bool RuleMatches(TilingRule rule, Vector3Int position, ITilemap tilemap, ref Matrix4x4 matrix4)
        {
            // Check rule against rotations of 0, 90, 180, 270
            for (var angle = 0;
                angle <= (rule.mAutoTransform == TilingRule.AutoTransform.Rotated ? 270 : 0);
                angle += 90)
            {
                if (!RuleMatches(rule, position, tilemap, angle)) continue;
                matrix4 = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, -angle), Vector3.one);
                return true;
            }

            // Check rule against x-axis mirror
            if ((rule.mAutoTransform == TilingRule.AutoTransform.MirrorX) &&
                RuleMatches(rule, position, tilemap, true, false))
            {
                matrix4 = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(-1f, 1f, 1f));
                return true;
            }

            // Check rule against y-axis mirror
            if ((rule.mAutoTransform != TilingRule.AutoTransform.MirrorY) ||
                !RuleMatches(rule, position, tilemap, false, true)) return false;
            matrix4 = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1f, -1f, 1f));
            return true;
        }

        public bool RuleMatches(TilingRule rule, Vector3Int position, ITilemap tilemap, int angle)
        {
            for (var y = -1; y <= 1; y++)
            {
                for (var x = -1; x <= 1; x++)
                {
                    if (x == 0 && y == 0) continue;
                    var offset = new Vector3Int(x, y, 0);
                    var rotated = GetRotatedPos(offset, angle);
                    var index = GetIndexOfOffset(rotated);
                    var tile = tilemap.GetTile(position + offset);
                    if (rule.mNeighbors[index] == TilingRule.Neighbor.This && tile != this ||
                        rule.mNeighbors[index] == TilingRule.Neighbor.NotThis && tile == this)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public bool RuleMatches(TilingRule rule, Vector3Int position, ITilemap tilemap, bool mirrorX, bool mirrorY)
        {
            for (var y = -1; y <= 1; y++)
            {
                for (var x = -1; x <= 1; x++)
                {
                    if (x == 0 && y == 0) continue;
                    var offset = new Vector3Int(x, y, 0);
                    var mirrored = GetMirroredPos(offset, mirrorX, mirrorY);
                    var index = GetIndexOfOffset(mirrored);
                    var tile = tilemap.GetTile(position + offset);
                    if (rule.mNeighbors[index] == TilingRule.Neighbor.This && tile != this ||
                        rule.mNeighbors[index] == TilingRule.Neighbor.NotThis && tile == this)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private int GetIndexOfOffset(Vector3Int offset)
        {
            var result = offset.x + 1 + (-offset.y + 1) * 3;
            if (result >= 4)
                result--;
            return result;
        }

        public Vector3Int GetRotatedPos(Vector3Int original, int rotation)
        {
            switch (rotation)
            {
                case 0:
                    return original;
                case 90:
                    return new Vector3Int(-original.y, original.x, original.z);
                case 180:
                    return new Vector3Int(-original.x, -original.y, original.z);
                case 270:
                    return new Vector3Int(original.y, -original.x, original.z);
            }

            return original;
        }

        public Vector3Int GetMirroredPos(Vector3Int original, bool mirrorX, bool mirrorY)
        {
            return new Vector3Int(original.x * (mirrorX ? -1 : 1), original.y * (mirrorY ? -1 : 1), original.z);
        }
    }
}