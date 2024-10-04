using System.Collections.Generic;

namespace Services.Models
{
    public class TreeNode
    {
        public List<TreeNode> Children { get; set; } = new List<TreeNode>();
        public required string Name { get; set; }
        public required string RelativePath { get; set; }
    }
}
