using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace War2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Battle battle = new Battle();
            battle.Execute();
        }
    }

    static class UserUtills
    {
        private static Random s_random = new Random();

        public static int ReturnPositiveRandomNumber(int maxValue)
        {
            return s_random.Next(maxValue);
        }

        public static int ReturnRandomNumber(int minValue, int maxValue)
        {
            return s_random.Next(minValue, maxValue);
        }
    }

    class Battle
    {
        private Platoon _platoon1 = new Platoon();
        private Platoon _platoon2 = new Platoon();

        public void Execute()
        {
            Console.WriteLine("Да начнётся бой!\n");

            while (_platoon1.FightersCount > 0 && _platoon2.FightersCount > 0)
            {
                _platoon1.Attack(_platoon2);
                _platoon2.Attack(_platoon1);
            }

            HonoringWinner();
        }

        private void HonoringWinner()
        {
            if (_platoon1.FightersCount > 0)
            {
                Console.WriteLine("Победил Взвод номер " + _platoon1.ID);
            }
            else if (_platoon2.FightersCount > 0)
            {
                Console.WriteLine("Победил Взвод номер " + _platoon2.ID);
            }
            else
            {
                Console.WriteLine("Победила дружба! Или проиграла.. Короче живых ни у кого нет.");
            }
        }
    }

    class Platoon
    {
        private static int s_count = 0;

        private List<IDamageble> _fighters = new List<IDamageble>();
        private Creator _creator = new Creator();

        public Platoon()
        {
            s_count++;
            CreateFighters();
            ID = s_count;
        }


        public int ID { get; }
        public int FightersCount => _fighters.Count;

        public void Attack(Platoon enemyPlatoon)
        {
            RemoveDeadFighters();

            Console.WriteLine("\nХод взвода номер " + ID);

            foreach (Fighter fighter in _fighters)
            {
                fighter.MakeTurn(enemyPlatoon);
            }
        }

        public List<IDamageble> ReturnAllFighters()
        {
            return _fighters.ToList();
        }

        private void RemoveDeadFighters()
        {
            List<IDamageble> deadFighters = new List<IDamageble>();

            foreach (Fighter fighter in _fighters)
            {
                if (fighter.IsDead)
                {
                    deadFighters.Add(fighter);
                }
            }

            foreach (Fighter fighter in deadFighters)
            {
                Console.WriteLine($"\nВзвод {ID} потерял {fighter.Name}");
                _fighters.Remove(fighter);
            }
        }

        private void CreateFighters()
        {
            _fighters.Add(_creator.GetFighter());
            _fighters.Add(_creator.GetCriticalWarrior());
            _fighters.Add(_creator.GetElectricMage());
            _fighters.Add(_creator.GetFireMage());
        }
    }

    interface IDamageble
    {
        void TakeDamage(int damage);
    }

    class Creator
    {
        private int _minHealth = 80;
        private int _maxHealth = 120;
        private int _minDamage = 15;
        private int _maxDamage = 20;
        private int _minArmor = 5;
        private int _maxArmor = 10;
        private int _maxTargets = 3;
        private string _criticalFighterName = "Боец с критическим уроном";
        private string _electrigMageName = "Электро-маг";
        private string _fireMageName = "Огненный маг";

        private int GetRandomHealth()
        {
            return UserUtills.ReturnRandomNumber(_minHealth, _maxHealth);
        }

        private int GetRandomDamage()
        {
            return UserUtills.ReturnRandomNumber(_minDamage, _maxDamage);
        }

        private int GetRandomArmor()
        {
            return UserUtills.ReturnRandomNumber(_minArmor, _maxArmor);
        }

        public Fighter GetFighter() => new Fighter(GetRandomHealth(), GetRandomDamage(), GetRandomArmor());
        public Fighter GetCriticalWarrior() => new CriticalFighter(GetRandomHealth(), GetRandomDamage(), GetRandomArmor(), _criticalFighterName);
        public Fighter GetElectricMage() => new ElectricMage(GetRandomHealth(), GetRandomDamage(), GetRandomArmor(), _electrigMageName, _maxTargets);
        public Fighter GetFireMage() => new FireMage(GetRandomHealth(), GetRandomDamage(), GetRandomArmor(), _fireMageName, _maxTargets);
    }

    class Fighter : IDamageble
    {
        public Fighter(int health, int damage, int armor, string name = "Рядовой боец")
        {
            Health = health;
            Damage = damage;
            Armor = armor;
            Name = name;
            MaxTargets = 1;
        }

        public Fighter(int health, int damage, int armor, string name, int targetsCount)
        {
            Health = health;
            Damage = damage;
            Armor = armor;
            Name = name;
            MaxTargets = targetsCount;
        }

        public bool IsDead => Health <= 0;

        protected internal string Name { get; }
        protected int Health { get; private set; }
        protected int Damage { get; }
        protected int Armor { get; }
        protected int MaxTargets { get; }


        public void MakeTurn(Platoon enemyPlatoon)
        {
            Attack(SelectTargets(enemyPlatoon));
        }

        public void TakeDamage(int damage)
        {
            Console.WriteLine($"{Name} получает {damage} урона.");
            Health -= (damage - Armor);
        }

        protected virtual void Attack(List<IDamageble> targets)
        {
            Console.WriteLine($"\n{Name} атакует!");
            foreach (IDamageble target in targets)
            {
                target.TakeDamage(Damage);
            }
        }

        protected virtual List<IDamageble> SelectTargets(Platoon platoon)
        {
            List<IDamageble> selectedTargets = new List<IDamageble>();

            selectedTargets.Add(platoon.ReturnAllFighters()[UserUtills.ReturnPositiveRandomNumber(platoon.ReturnAllFighters().Count)]);

            return selectedTargets;
        }
    }

    class CriticalFighter : Fighter
    {
        private readonly int _damageFactor = 2;

        public CriticalFighter(int health, int damage, int armor, string name) : base(health, damage, armor, name)
        {

        }

        protected override void Attack(List<IDamageble> targets)
        {
            Console.WriteLine($"\n{Name} атакует!");

            foreach (IDamageble target in targets)
            {
                target.TakeDamage(Damage * _damageFactor);
            }
        }
    }

    class ElectricMage : Fighter
    {
        public ElectricMage(int health, int damage, int armor, string name, int targetsCount) : base(health, damage, armor, name, targetsCount)
        {

        }

        protected override List<IDamageble> SelectTargets(Platoon platoon)
        {
            List<IDamageble> potencialTargets = platoon.ReturnAllFighters();
            List<IDamageble> selectedTargets = new List<IDamageble>();
            IDamageble enemyFighter;

            while (potencialTargets.Count > 0 && selectedTargets.Count < MaxTargets)
            {
                enemyFighter = potencialTargets[UserUtills.ReturnPositiveRandomNumber(potencialTargets.Count)];
                selectedTargets.Add(enemyFighter);
                potencialTargets.Remove(enemyFighter);
            }

            return selectedTargets;
        }

        protected override void Attack(List<IDamageble> target)
        {
            base.Attack(target);
        }
    }

    class FireMage : Fighter
    {
        public FireMage(int health, int damage, int armor, string name, int targetsCount) : base(health, damage, armor, name, targetsCount)
        {

        }

        protected override List<IDamageble> SelectTargets(Platoon platoon)
        {
            List<IDamageble> selectedTargets = new List<IDamageble>();
            IDamageble enemyFighter;

            while (platoon.FightersCount > 0 && selectedTargets.Count < MaxTargets)
            {
                enemyFighter = platoon.ReturnAllFighters()[UserUtills.ReturnPositiveRandomNumber(platoon.FightersCount)];
                selectedTargets.Add(enemyFighter);
            }

            return selectedTargets;
        }
    }
}
