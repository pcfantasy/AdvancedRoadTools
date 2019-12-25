using AdvancedRoadTools.Util;
using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AdvancedRoadTools.NewData
{
	public static class CustomNetSegment
	{
		public static float[] segmentOffset = new float[8];
		public static void CalculateMiddlePoints(Vector3 startPos, Vector3 startDir, Vector3 endPos, Vector3 endDir, bool smoothStart, bool smoothEnd, out Vector3 middlePos1, out Vector3 middlePos2)
		{
			if (IsStraight(startPos, startDir, endPos, endDir, out float distance))
			{
				middlePos1 = startPos + startDir * (distance * ((!smoothStart) ? 0.15f : 0.276f));
				middlePos2 = endPos + endDir * (distance * ((!smoothEnd) ? 0.15f : 0.276f));
			}
			else
			{
				float num = startDir.x * endDir.x + startDir.z * endDir.z;
				if (num >= -0.999f && Line2.Intersect(VectorUtils.XZ(startPos), VectorUtils.XZ(startPos + startDir), VectorUtils.XZ(endPos), VectorUtils.XZ(endPos + endDir), out float u, out float v))
				{
					u = Mathf.Clamp(u, distance * 0.1f, distance);
					v = Mathf.Clamp(v, distance * 0.1f, distance);
					float num2 = u + v;
					middlePos1 = startPos + startDir * Mathf.Min(u, num2 * 0.276f);
					middlePos2 = endPos + endDir * Mathf.Min(v, num2 * 0.276f);
				}
				else
				{
					middlePos1 = startPos + startDir * (distance * 0.276f);
					middlePos2 = endPos + endDir * (distance * 0.276f);
				}
			}
		}

		public static bool IsStraight(Vector3 startPos, Vector3 startDir, Vector3 endPos, Vector3 endDir, out float distance)
		{
			Vector3 vector = VectorUtils.NormalizeXZ(endPos - startPos, out distance);
			float num = startDir.x * endDir.x + startDir.z * endDir.z;
			float num2 = startDir.x * vector.x + startDir.z * vector.z;
			return num < -0.999f && num2 > 0.999f;
		}

		public static void CalculateCorner(ref NetSegment segment, ushort segmentID, bool heightOffset, bool start, bool leftSide, out Vector3 cornerPos, out Vector3 cornerDirection, out bool smooth)
		{
			NetInfo info = segment.Info;
			NetManager instance = Singleton<NetManager>.instance;
			ushort num = (!start) ? segment.m_endNode : segment.m_startNode;
			ushort num2 = (!start) ? segment.m_startNode : segment.m_endNode;
			Vector3 position = instance.m_nodes.m_buffer[(int)num].m_position;
			Vector3 position2 = instance.m_nodes.m_buffer[(int)num2].m_position;
			Vector3 startDir = (!start) ? segment.m_endDirection : segment.m_startDirection;
			Vector3 endDir = (!start) ? segment.m_startDirection : segment.m_endDirection;
			// NON-STOCK CODE STARTS
			float m_minCornerOffset = 0f;
			for (int i = 0; i < 8; i++)
			{
				ushort segment1 = instance.m_nodes.m_buffer[num].GetSegment(i);
				if (segment1 == segmentID)
				{
					m_minCornerOffset = MainDataStore.segmentModifiedMinOffset[num * 8 + i];
				}
			}
			// NON-STOCK CODE END
			CalculateCorner(m_minCornerOffset, info, position, position2, startDir, endDir, null, Vector3.zero, Vector3.zero, Vector3.zero, null, Vector3.zero, Vector3.zero, Vector3.zero, segmentID, num, heightOffset, leftSide, out cornerPos, out cornerDirection, out smooth);
		}

		public static void CalculateCorner(float minCornerOffset, NetInfo info, Vector3 startPos, Vector3 endPos, Vector3 startDir, Vector3 endDir, NetInfo extraInfo1, Vector3 extraEndPos1, Vector3 extraStartDir1, Vector3 extraEndDir1, NetInfo extraInfo2, Vector3 extraEndPos2, Vector3 extraStartDir2, Vector3 extraEndDir2, ushort ignoreSegmentID, ushort startNodeID, bool heightOffset, bool leftSide, out Vector3 cornerPos, out Vector3 cornerDirection, out bool smooth)
		{
			NetManager instance = Singleton<NetManager>.instance;
			Bezier3 bezier = default(Bezier3);
			Bezier3 bezier2 = default(Bezier3);
			NetNode.Flags flags = NetNode.Flags.End;
			ushort num = 0;
			if (startNodeID != 0)
			{
				flags = instance.m_nodes.m_buffer[startNodeID].m_flags;
				num = instance.m_nodes.m_buffer[startNodeID].m_building;
			}
			cornerDirection = startDir;
			float d = (!leftSide) ? (0f - info.m_halfWidth) : info.m_halfWidth;
			smooth = ((flags & NetNode.Flags.Middle) != NetNode.Flags.None);
			if ((object)extraInfo1 != null)
			{
				flags = (((flags & NetNode.Flags.End) == NetNode.Flags.None || !info.IsCombatible(extraInfo1) || (object)extraInfo2 != null) ? ((flags & ~(NetNode.Flags.Middle | NetNode.Flags.Bend)) | NetNode.Flags.Junction) : ((!(startDir.x * extraStartDir1.x + startDir.z * extraStartDir1.z < -0.999f)) ? ((flags & ~NetNode.Flags.End) | NetNode.Flags.Bend) : ((flags & ~NetNode.Flags.End) | NetNode.Flags.Middle)));
			}
			if ((flags & NetNode.Flags.Middle) != 0)
			{
				int num2 = ((object)extraInfo1 != null) ? (-1) : 0;
				int num3 = (startNodeID != 0) ? 8 : 0;
				int num4 = num2;
				while (true)
				{
					if (num4 < num3)
					{
						Vector3 b;
						if (num4 == -1)
						{
							b = extraStartDir1;
						}
						else
						{
							ushort segment = instance.m_nodes.m_buffer[startNodeID].GetSegment(num4);
							if (segment == 0 || segment == ignoreSegmentID)
							{
								num4++;
								continue;
							}
							ushort startNode = instance.m_segments.m_buffer[segment].m_startNode;
							b = ((startNodeID == startNode) ? instance.m_segments.m_buffer[segment].m_startDirection : instance.m_segments.m_buffer[segment].m_endDirection);
						}
						cornerDirection = VectorUtils.NormalizeXZ(cornerDirection - b);
					}
					break;
				}
			}
			Vector3 vector = Vector3.Cross(cornerDirection, Vector3.up).normalized;
			if (info.m_twistSegmentEnds)
			{
				if (num != 0)
				{
					float angle = Singleton<BuildingManager>.instance.m_buildings.m_buffer[num].m_angle;
					Vector3 vector2 = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
					vector = ((!(Vector3.Dot(vector, vector2) >= 0f)) ? (-vector2) : vector2);
				}
				else if ((flags & NetNode.Flags.Junction) != 0 && startNodeID != 0)
				{
					Vector3 vector3 = Vector3.zero;
					int num5 = 0;
					for (int i = 0; i < 8; i++)
					{
						ushort segment2 = instance.m_nodes.m_buffer[startNodeID].GetSegment(i);
						if (segment2 != 0 && segment2 != ignoreSegmentID && (instance.m_segments.m_buffer[segment2].m_flags & NetSegment.Flags.Untouchable) != 0)
						{
							Vector3 vector4 = (instance.m_segments.m_buffer[segment2].m_startNode == startNodeID) ? instance.m_segments.m_buffer[segment2].m_startDirection : instance.m_segments.m_buffer[segment2].m_endDirection;
							vector3 = new Vector3(vector4.z, 0f, 0f - vector4.x);
							num5++;
						}
					}
					if (num5 == 1)
					{
						vector = ((!(Vector3.Dot(vector, vector3) >= 0f)) ? (-vector3) : vector3);
					}
				}
			}
			bezier.a = startPos + vector * d;
			bezier2.a = startPos - vector * d;
			cornerPos = bezier.a;
			if (((flags & NetNode.Flags.Junction) != 0 && info.m_clipSegmentEnds) || (flags & (NetNode.Flags.Bend | NetNode.Flags.Outside)) != 0)
			{
				vector = Vector3.Cross(endDir, Vector3.up).normalized;
				bezier.d = endPos - vector * d;
				bezier2.d = endPos + vector * d;
				NetSegment.CalculateMiddlePoints(bezier.a, cornerDirection, bezier.d, endDir, false, false, out bezier.b, out bezier.c);
				NetSegment.CalculateMiddlePoints(bezier2.a, cornerDirection, bezier2.d, endDir, false, false, out bezier2.b, out bezier2.c);
				Bezier2 bezier3 = Bezier2.XZ(bezier);
				Bezier2 bezier4 = Bezier2.XZ(bezier2);
				float num6 = -1f;
				float num7 = -1f;
				bool flag = false;
				int num8 = ((object)extraInfo1 != null) ? (((object)extraInfo2 == null) ? (-1) : (-2)) : 0;
				int num9 = (startNodeID != 0) ? 8 : 0;
				float a = info.m_halfWidth * 0.5f;
				int num10 = 0;
				for (int j = num8; j < num9; j++)
				{
					NetInfo netInfo;
					Vector3 vector5;
					switch (j)
					{
						case -2:
							netInfo = extraInfo2;
							vector5 = extraStartDir2;
							if (extraEndPos2 == endPos && extraEndDir2 == endDir)
							{
								break;
							}
							goto IL_05e9;
						case -1:
							netInfo = extraInfo1;
							vector5 = extraStartDir1;
							if (extraEndPos1 == endPos && extraEndDir1 == endDir)
							{
								break;
							}
							goto IL_05e9;
						default:
							{
								ushort segment3 = instance.m_nodes.m_buffer[startNodeID].GetSegment(j);
								if (segment3 == 0 || segment3 == ignoreSegmentID)
								{
									break;
								}
								netInfo = instance.m_segments.m_buffer[segment3].Info;
								vector5 = instance.m_segments.m_buffer[segment3].GetDirection(startNodeID);
								goto IL_05e9;
							}
						IL_05e9:
							if ((object)netInfo != null && info.m_clipSegmentEnds == netInfo.m_clipSegmentEnds)
							{
								if (netInfo.m_netAI.GetSnapElevation() > info.m_netAI.GetSnapElevation())
								{
									float num11 = 0.01f - Mathf.Min(info.m_maxTurnAngleCos, netInfo.m_maxTurnAngleCos);
									float num12 = vector5.x * startDir.x + vector5.z * startDir.z;
									if ((info.m_vehicleTypes & netInfo.m_vehicleTypes) == VehicleInfo.VehicleType.None || num12 >= num11)
									{
										break;
									}
								}
								a = Mathf.Max(a, netInfo.m_halfWidth * 0.5f);
								num10++;
							}
							break;
					}
				}
				if (num10 >= 1 || (flags & NetNode.Flags.Outside) != 0)
				{
					for (int k = num8; k < num9; k++)
					{
						NetInfo netInfo2;
						Vector3 vector9;
						Vector3 vector6;
						Vector3 vector7;
						switch (k)
						{
							case -2:
								netInfo2 = extraInfo2;
								vector9 = extraEndPos2;
								vector6 = extraStartDir2;
								vector7 = extraEndDir2;
								if (vector9 == endPos && vector7 == endDir)
								{
									break;
								}
								goto IL_082e;
							case -1:
								netInfo2 = extraInfo1;
								vector9 = extraEndPos1;
								vector6 = extraStartDir1;
								vector7 = extraEndDir1;
								if (vector9 == endPos && vector7 == endDir)
								{
									break;
								}
								goto IL_082e;
							default:
								{
									ushort segment4 = instance.m_nodes.m_buffer[startNodeID].GetSegment(k);
									if (segment4 == 0 || segment4 == ignoreSegmentID)
									{
										break;
									}
									ushort startNode2 = instance.m_segments.m_buffer[segment4].m_startNode;
									ushort num13 = instance.m_segments.m_buffer[segment4].m_endNode;
									vector6 = instance.m_segments.m_buffer[segment4].m_startDirection;
									vector7 = instance.m_segments.m_buffer[segment4].m_endDirection;
									if (startNodeID != startNode2)
									{
										ushort num14 = startNode2;
										startNode2 = num13;
										num13 = num14;
										Vector3 vector8 = vector6;
										vector6 = vector7;
										vector7 = vector8;
									}
									netInfo2 = instance.m_segments.m_buffer[segment4].Info;
									vector9 = instance.m_nodes.m_buffer[num13].m_position;
									goto IL_082e;
								}
							IL_082e:
								if ((object)netInfo2 != null && info.m_clipSegmentEnds == netInfo2.m_clipSegmentEnds)
								{
									if (netInfo2.m_netAI.GetSnapElevation() > info.m_netAI.GetSnapElevation())
									{
										float num15 = 0.01f - Mathf.Min(info.m_maxTurnAngleCos, netInfo2.m_maxTurnAngleCos);
										float num16 = vector6.x * startDir.x + vector6.z * startDir.z;
										if ((info.m_vehicleTypes & netInfo2.m_vehicleTypes) == VehicleInfo.VehicleType.None || num16 >= num15)
										{
											break;
										}
									}
									if (vector6.z * cornerDirection.x - vector6.x * cornerDirection.z > 0f == leftSide)
									{
										Bezier3 bezier5 = default(Bezier3);
										float num17 = Mathf.Max(a, netInfo2.m_halfWidth);
										if (!leftSide)
										{
											num17 = 0f - num17;
										}
										vector = Vector3.Cross(vector6, Vector3.up).normalized;
										bezier5.a = startPos - vector * num17;
										vector = Vector3.Cross(vector7, Vector3.up).normalized;
										bezier5.d = vector9 + vector * num17;
										NetSegment.CalculateMiddlePoints(bezier5.a, vector6, bezier5.d, vector7, false, false, out bezier5.b, out bezier5.c);
										Bezier2 b2 = Bezier2.XZ(bezier5);
										if (bezier3.Intersect(b2, out float t, out float t2, 6))
										{
											num6 = Mathf.Max(num6, t);
										}
										else if (bezier3.Intersect(b2.a, b2.a - VectorUtils.XZ(vector6) * 16f, out t, out t2, 6))
										{
											num6 = Mathf.Max(num6, t);
										}
										else if (b2.Intersect(bezier3.d + (bezier3.d - bezier4.d) * 0.01f, bezier4.d, out t, out t2, 6))
										{
											num6 = Mathf.Max(num6, 1f);
										}
										float num18 = cornerDirection.x * vector6.x + cornerDirection.z * vector6.z;
										if (num18 >= -0.75f)
										{
											flag = true;
										}
									}
									else
									{
										Bezier3 bezier6 = default(Bezier3);
										float num19 = cornerDirection.x * vector6.x + cornerDirection.z * vector6.z;
										if (num19 >= 0f)
										{
											vector6.x -= cornerDirection.x * num19 * 2f;
											vector6.z -= cornerDirection.z * num19 * 2f;
										}
										float num20 = Mathf.Max(a, netInfo2.m_halfWidth);
										if (!leftSide)
										{
											num20 = 0f - num20;
										}
										vector = Vector3.Cross(vector6, Vector3.up).normalized;
										bezier6.a = startPos + vector * num20;
										vector = Vector3.Cross(vector7, Vector3.up).normalized;
										bezier6.d = vector9 - vector * num20;
										NetSegment.CalculateMiddlePoints(bezier6.a, vector6, bezier6.d, vector7, false, false, out bezier6.b, out bezier6.c);
										Bezier2 b3 = Bezier2.XZ(bezier6);
										if (bezier4.Intersect(b3, out float t3, out float t4, 6))
										{
											num7 = Mathf.Max(num7, t3);
										}
										else if (bezier4.Intersect(b3.a, b3.a - VectorUtils.XZ(vector6) * 16f, out t3, out t4, 6))
										{
											num7 = Mathf.Max(num7, t3);
										}
										else if (b3.Intersect(bezier3.d, bezier4.d + (bezier4.d - bezier3.d) * 0.01f, out t3, out t4, 6))
										{
											num7 = Mathf.Max(num7, 1f);
										}
									}
								}
								break;
						}
					}
					if ((flags & NetNode.Flags.Junction) != 0)
					{
						if (!flag)
						{
							num6 = Mathf.Max(num6, num7);
						}
					}
					else if ((flags & NetNode.Flags.Bend) != 0 && !flag)
					{
						num6 = Mathf.Max(num6, num7);
					}
					if ((flags & NetNode.Flags.Outside) != 0)
					{
						float num21 = 8640f;
						Vector2 vector10 = new Vector2(0f - num21, 0f - num21);
						Vector2 vector11 = new Vector2(0f - num21, num21);
						Vector2 vector12 = new Vector2(num21, num21);
						Vector2 vector13 = new Vector2(num21, 0f - num21);
						if (bezier3.Intersect(vector10, vector11, out float t5, out float t6, 6))
						{
							num6 = Mathf.Max(num6, t5);
						}
						if (bezier3.Intersect(vector11, vector12, out t5, out t6, 6))
						{
							num6 = Mathf.Max(num6, t5);
						}
						if (bezier3.Intersect(vector12, vector13, out t5, out t6, 6))
						{
							num6 = Mathf.Max(num6, t5);
						}
						if (bezier3.Intersect(vector13, vector10, out t5, out t6, 6))
						{
							num6 = Mathf.Max(num6, t5);
						}
						num6 = Mathf.Clamp01(num6);
					}
					else
					{
						if (num6 < 0f)
						{
							num6 = ((!(info.m_halfWidth < 4f)) ? bezier3.Travel(0f, 8f) : 0f);
						}
						float num22 = info.m_minCornerOffset;
						// NON-STOCK CODE STARTS
						if (minCornerOffset != 0)
						{
							num22 = minCornerOffset;
						}
						// NON-STOCK CODE END
						if ((flags & (NetNode.Flags.AsymForward | NetNode.Flags.AsymBackward)) != 0)
						{
						    num22 = Mathf.Max(num22, 8f);
						}
						num6 = Mathf.Clamp01(num6);
						float num23 = VectorUtils.LengthXZ(bezier.Position(num6) - bezier.a);
						num6 = bezier3.Travel(num6, Mathf.Max(num22 - num23, 2f));
						if (info.m_straightSegmentEnds)
						{
							if (num7 < 0f)
							{
								num7 = ((!(info.m_halfWidth < 4f)) ? bezier4.Travel(0f, 8f) : 0f);
							}
							num7 = Mathf.Clamp01(num7);
							num23 = VectorUtils.LengthXZ(bezier2.Position(num7) - bezier2.a);
							// NON-STOCK CODE STARTS
							if (minCornerOffset != 0)
								num7 = bezier4.Travel(num7, Mathf.Max(minCornerOffset - num23, 2f));
						    else
							    num7 = bezier4.Travel(num7, Mathf.Max(info.m_minCornerOffset - num23, 2f));
							// NON-STOCK CODE END
							num6 = Mathf.Max(num6, num7);
						}
					}
					float y = cornerDirection.y;
					cornerDirection = bezier.Tangent(num6);
					cornerDirection.y = 0f;
					cornerDirection.Normalize();
					if (!info.m_flatJunctions)
					{
						cornerDirection.y = y;
					}
					cornerPos = bezier.Position(num6);
					cornerPos.y = startPos.y;
				}
			}
			else if (((flags & NetNode.Flags.Junction) != 0 && info.m_minCornerOffset >= 0.01f) || ((flags & NetNode.Flags.Junction) != 0 && minCornerOffset != 0))
			{
				vector = Vector3.Cross(endDir, Vector3.up).normalized;
				bezier.d = endPos - vector * d;
				bezier2.d = endPos + vector * d;
				NetSegment.CalculateMiddlePoints(bezier.a, cornerDirection, bezier.d, endDir, false, false, out bezier.b, out bezier.c);
				NetSegment.CalculateMiddlePoints(bezier2.a, cornerDirection, bezier2.d, endDir, false, false, out bezier2.b, out bezier2.c);
				Bezier2 bezier7 = Bezier2.XZ(bezier);
				Bezier2 bezier8 = Bezier2.XZ(bezier2);
				float value = (!(info.m_halfWidth < 4f)) ? bezier7.Travel(0f, 8f) : 0f;
				value = Mathf.Clamp01(value);
				float num24 = VectorUtils.LengthXZ(bezier.Position(value) - bezier.a);
				// NON-STOCK CODE STARTS
				if (minCornerOffset != 0)
					value = bezier7.Travel(value, Mathf.Max(minCornerOffset - num24, 2f));
				else
					value = bezier7.Travel(value, Mathf.Max(info.m_minCornerOffset - num24, 2f));
				// NON-STOCK CODE END
				if (info.m_straightSegmentEnds)
				{
					float value2 = (!(info.m_halfWidth < 4f)) ? bezier8.Travel(0f, 8f) : 0f;
					value2 = Mathf.Clamp01(value2);
					num24 = VectorUtils.LengthXZ(bezier2.Position(value2) - bezier2.a);
					// NON-STOCK CODE STARTS
					if (minCornerOffset != 0)
						value2 = bezier7.Travel(value2, Mathf.Max(minCornerOffset - num24, 2f));
					else
						value2 = bezier8.Travel(value2, Mathf.Max(info.m_minCornerOffset - num24, 2f));
					// NON-STOCK CODE END
					value = Mathf.Max(value, value2);
				}
				float y2 = cornerDirection.y;
				cornerDirection = bezier.Tangent(value);
				cornerDirection.y = 0f;
				cornerDirection.Normalize();
				if (!info.m_flatJunctions)
				{
					cornerDirection.y = y2;
				}
				cornerPos = bezier.Position(value);
				cornerPos.y = startPos.y;
			}
			if (heightOffset && startNodeID != 0)
			{
				cornerPos.y += (float)(int)instance.m_nodes.m_buffer[startNodeID].m_heightOffset * 0.015625f;
			}
		}
	}
}

