using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.EventHandlers {
    public class Revision {

        public const long InitialVersion = 1L;

        public static readonly Revision Empty = new Revision(0L, 0L);

        private readonly long _id;
        private readonly long _version;

        private Revision(long id, long version) {
            _id = id;
            _version = version; 
        }

        public static Revision Create(long id) {
            return new Revision(id, Revision.InitialVersion);
        }

        public static Revision Update(long id, long version) {
            return new Revision(id, version);
        }

        public static bool IsNullOrEmpty(Revision revision) {
            return ((revision == null) || revision.IsEmpty);
        }

        #region Overrides

        public override string ToString() {
            return String.Format("({0} v{1})", _id, _version);
        }

        public override bool Equals(object obj) {

            var equal = false;
            var other = (obj as Revision);

            if(other != null) {
                equal = this.Id == other.Id && this.Version == other.Version;
            }

            return equal;
        }

        public override int GetHashCode() {
            return (3 * _id.GetHashCode()) + (5 * _version.GetHashCode());
        }

        #endregion

        #region Properties

        public long Id { get { return _id; } }
        public long Version { get { return _version; } }
        public bool IsEmpty { get { return _id == 0L && _version == 0L; } }

        #endregion

    }
}
