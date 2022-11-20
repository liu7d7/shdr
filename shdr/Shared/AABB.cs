namespace shdr.Shared
{
   public struct __aabb
   {
      public float minX;
      public float minY;
      public float minZ;
      public float maxX;
      public float maxY;
      public float maxZ;

      public __aabb(float minX, float minY, float minZ, float maxX, float maxY, float maxZ)
      {
         this.minX = minX;
         this.minY = minY;
         this.minZ = minZ;
         this.maxX = maxX;
         this.maxY = maxY;
         this.maxZ = maxZ;
      }
   }
}