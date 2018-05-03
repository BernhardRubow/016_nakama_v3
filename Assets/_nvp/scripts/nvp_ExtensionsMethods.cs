using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ExtensionMethods
{

  public static class nvp_ExtensionMethods
  {
    public static List<float> AddVector(this List<float> list, Vector3 vector)
    {

      list.AddRange(new[] { vector.x, vector.y, vector.z });

      return list;
    }

    public static byte[] Serialize(this List<float> source)
    {
      // create a byte array and copy the floats into it...
      var byteArray = new byte[source.Count * sizeof(float)];
      System.Buffer.BlockCopy(source.ToArray(), 0, byteArray, 0, byteArray.Length);
      return byteArray;
    }

    public static List<float> Deserialize(this byte[] byteArray)
    {
      // create a second float array and copy the bytes into it...
      var floatArray2 = new float[byteArray.Length / 4];
      System.Buffer.BlockCopy(byteArray, 0, floatArray2, 0, byteArray.Length);
      return floatArray2.ToList();
    }
  }

}