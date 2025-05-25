using DeviceManager.Mobile.Models;
using Realms;
using System.Linq.Expressions;

namespace DeviceManager.Mobile.RealmDb
{
    public class RealmDbContext
    {
        private static Realm _realmInstance;

        public static Realm GetInstance()
        {
            if (_realmInstance == null)
            {
                var config = new RealmConfiguration("dispositivos.realm")
                {
                    SchemaVersion = 1,
                    ShouldDeleteIfMigrationNeeded = true
                };

                _realmInstance = Realm.GetInstance(config);
            }

            return _realmInstance;
        }

        public static bool IsCodeReferenceUnique(string codeReference)
        {
            using var realm = GetInstance();
            return !realm.All<Dispositivo>().Any(d => d.CodigoReferencia == codeReference);
        }

        public static void Add<T>(T entity) where T : RealmObject
        {
            using var realm = GetInstance();
            realm.Write(() =>
            {
                realm.Add(entity);
            });
        }

        public static void Update<T>(T entity) where T : RealmObject
        {
            using var realm = GetInstance();
            realm.Write(() =>
            {
                realm.Add(entity, update: true);
            });
        }

        public static void Delete<T>(T entity) where T : RealmObject
        {
            using var realm = GetInstance();
            realm.Write(() =>
            {
                realm.Remove(entity);
            });
        }

        public static IQueryable<T> GetAll<T>() where T : RealmObject
        {
            using var realm = GetInstance();
            return realm.All<T>();
        }

        public static T Find<T>(Expression<Func<T, bool>> predicate) where T : RealmObject
        {
            using var realm = GetInstance();
            return realm.All<T>().FirstOrDefault(predicate);
        }
    }
}
