using System;
using System.Collections.Generic;
using System.Linq;

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

    class Battle
    {
        private FighterCreator _fighterCreator;
        private Platoon _platoon1;
        private Platoon _platoon2;

        public Battle()
        {
            _fighterCreator = new FighterCreator();
            _platoon1 = new Platoon(_fighterCreator);
            _platoon2 = new Platoon(_fighterCreator);
        }

        public void Execute()
        {
            Console.WriteLine("Да начнётся бой!\n");

            while (_platoon1.FightersCount > 0 && _platoon2.FightersCount > 0)
            {
                _platoon1.Attack(_platoon2.GetAllFighters());
                _platoon2.Attack(_platoon1.GetAllFighters());

                _platoon1.RemoveDeadFighters();
                _platoon2.RemoveDeadFighters();
            }

            HonorWinner();
        }

        private void HonorWinner()
        {
            if (_platoon1.FightersCount > 0)
            {
                Console.WriteLine("Победил Взвод номер " + _platoon1.Id);
            }
            else if (_platoon2.FightersCount > 0)
            {
                Console.WriteLine("Победил Взвод номер " + _platoon2.Id);
            }
            else
            {
                Console.WriteLine("Победила дружба! Или проиграла.. Короче в живых никого нет.");
            }
        }
    }

    class Platoon
    {
        private static int s_count = 0;

        private List<Fighter> _fighters = new List<Fighter>();
        private FighterCreator _creator;

        public Platoon(FighterCreator creator)
        {
            s_count++;
            Id = s_count;
            _creator = creator;
            FillFighters();
        }

        public int Id { get; }
        public int FightersCount => _fighters.Count;

        public void Attack(List<Fighter> _enemyFighters)
        {
            Console.WriteLine("\nХод взвода номер " + Id);

            foreach (Fighter fighter in _fighters)
            {
                fighter.MakeTurn(_enemyFighters);
            }
        }

        public List<Fighter> GetAllFighters()
        {
            return _fighters.ToList();
        }

        public void RemoveDeadFighters()
        {
            List<Fighter> deadFighters = new List<Fighter>();

            foreach (Fighter fighter in _fighters)
            {
                if (fighter.IsDead)
                {
                    deadFighters.Add(fighter);
                }
            }

            foreach (Fighter fighter in deadFighters)
            {
                Console.WriteLine($"\nВзвод {Id} потерял {fighter.Name}");
                _fighters.Remove(fighter);
            }
        }

        private void FillFighters()
        {
            _fighters.Add(_creator.CreateFighter());
            _fighters.Add(_creator.CreateCriticalWarrior());
            _fighters.Add(_creator.CreateElectricMage());
            _fighters.Add(_creator.CreateFireMage());
        }
    }

    class FighterCreator
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

        private int GenerateRandomHealth()
        {
            return UserUtills.GenerateRandomNumber(_minHealth, _maxHealth);
        }

        private int GenerateRandomDamage()
        {
            return UserUtills.GenerateRandomNumber(_minDamage, _maxDamage);
        }

        private int GenerateRandomArmor()
        {
            return UserUtills.GenerateRandomNumber(_minArmor, _maxArmor);
        }

        public Fighter CreateFighter() => new Fighter(GenerateRandomHealth(), GenerateRandomDamage(), GenerateRandomArmor());
        public Fighter CreateCriticalWarrior() => new CriticalFighter(GenerateRandomHealth(), GenerateRandomDamage(), GenerateRandomArmor(), _criticalFighterName);
        public Fighter CreateElectricMage() => new ElectricMage(GenerateRandomHealth(), GenerateRandomDamage(), GenerateRandomArmor(), _electrigMageName, _maxTargets);
        public Fighter CreateFireMage() => new FireMage(GenerateRandomHealth(), GenerateRandomDamage(), GenerateRandomArmor(), _fireMageName, _maxTargets);
    }

    interface IDamageble
    {
        void TakeDamage(int damage);
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


        public void MakeTurn(List<Fighter> enemyFighters)
        {
            Attack(SelectTargets(enemyFighters));
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

        protected virtual List<IDamageble> SelectTargets(List<Fighter> enemyFighters)
        {
            List<IDamageble> selectedTargets = new List<IDamageble>();

            selectedTargets.Add(enemyFighters[UserUtills.GeneratePositiveRandomNumber(enemyFighters.Count)]);

            return selectedTargets;
        }
    }

    class CriticalFighter : Fighter
    {
        private readonly int _damageFactor = 2;

        public CriticalFighter(int health, int damage, int armor, string name) : base(health, damage, armor, name) { }

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
        public ElectricMage(int health, int damage, int armor, string name, int targetsCount) : base(health, damage, armor, name, targetsCount) { }

        protected override List<IDamageble> SelectTargets(List<Fighter> enemyFighters)
        {
            List<Fighter> potencialTargets = enemyFighters;
            List<IDamageble> selectedTargets = new List<IDamageble>();
            IDamageble enemyFighter;
            int selectedFighterIndex;

            while (potencialTargets.Count > 0 && selectedTargets.Count < MaxTargets)
            {
                selectedFighterIndex = UserUtills.GeneratePositiveRandomNumber(potencialTargets.Count);
                enemyFighter = potencialTargets[selectedFighterIndex];
                selectedTargets.Add(enemyFighter);
                potencialTargets.RemoveAt(selectedFighterIndex);
            }

            return selectedTargets;
        }
    }

    class FireMage : Fighter
    {
        public FireMage(int health, int damage, int armor, string name, int targetsCount) : base(health, damage, armor, name, targetsCount) { }

        protected override List<IDamageble> SelectTargets(List<Fighter> enemyFighters)
        {
            List<IDamageble> selectedTargets = new List<IDamageble>();
            IDamageble enemyFighter;
            int selectedFighterIndex;

            while (enemyFighters.Count > 0 && selectedTargets.Count < MaxTargets)
            {
                selectedFighterIndex = UserUtills.GeneratePositiveRandomNumber(enemyFighters.Count);
                enemyFighter = enemyFighters[selectedFighterIndex];
                selectedTargets.Add(enemyFighter);
            }

            return selectedTargets;
        }
    }

    static class UserUtills
    {
        private static Random s_random = new Random();

        public static int GeneratePositiveRandomNumber(int maxValue)
        {
            return s_random.Next(maxValue);
        }

        public static int GenerateRandomNumber(int minValue, int maxValue)
        {
            return s_random.Next(minValue, maxValue);
        }
    }
}
