using System.Collections.Generic;

public static class SaveMapper
    {
        public static GameSaveData ToDto(FactoryModel model, float boostTimeRemaining)
        {
            var dto = new GameSaveData
            {
                balance = model.Balance,
                boostTimeRemaining = boostTimeRemaining,
                machines = new List<MachineSaveData>(model.Machines.Count)
            };

            for (int i = 0; i < model.Machines.Count; i++)
            {
                var m = model.Machines[i];
                dto.machines.Add(new MachineSaveData
                {
                    id = m.Config.Id,
                    level = m.Level,
                    isUnlocked = m.IsUnlocked
                });
            }
            return dto;
        }

        public static void Apply(GameSaveData dto, FactoryModel model)
        {
            if (dto == null || model == null) return;

            model.RestoreBalance(dto.balance);

            if (dto.machines == null || dto.machines.Count == 0) return;

            var lookup = new Dictionary<string, MachineSaveData>(dto.machines.Count);
            for (int i = 0; i < dto.machines.Count; i++)
            {
                var s = dto.machines[i];
                if (s == null || string.IsNullOrEmpty(s.id)) continue;
                lookup[s.id] = s;
            }

            for (int i = 0; i < model.Machines.Count; i++)
            {
                var m = model.Machines[i];
                if (lookup.TryGetValue(m.Config.Id, out var s))
                    m.Restore(s.level, s.isUnlocked);
            }
        }
    }