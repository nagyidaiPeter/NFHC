using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NfhcLauncher.Patching
{
    internal class NfhcEntryPatch
    {
        //1 341 952
        //1 338 880
        public const string GAME_ASSEMBLY_NAME = "Assembly-CSharp.dll";
        public const string PoCoopASSEMBLY_NAME = "NfhcBootloader.dll";
        public const string GAME_ASSEMBLY_MODIFIED_NAME = "Assembly-CSharp-Nfhc.dll";

        private const string PoCoopENTRY_TYPE_NAME = "Main";
        private const string PoCoopENTRY_METHOD_NAME = "Execute";

        private const string GAME_INPUT_TYPE_NAME = "NFH.Game.PlatformController";
        private const string GAME_INPUT_METHOD_NAME = "Awake";

        private const string PoCoop_EXECUTE_INSTRUCTION = "System.Void NfhBootloader.Main::Execute()";

        private readonly string NfhcManagedPath;

        public bool IsApplied => IsPatchApplied();

        public NfhcEntryPatch(string NfhcBasePath)
        {
            NfhcManagedPath = Path.Combine(NfhcBasePath, "Neighbours back From Hell_Data", "Managed");
        }

        public void Apply()
        {
            string assemblyCSharp = Path.Combine(NfhcManagedPath, GAME_ASSEMBLY_NAME);
            string NfhcPatcherPath = Path.Combine(NfhcManagedPath, PoCoopASSEMBLY_NAME);
            string modifiedAssemblyCSharp = Path.Combine(NfhcManagedPath, GAME_ASSEMBLY_MODIFIED_NAME);

            if (File.Exists(modifiedAssemblyCSharp))
            {
                File.Delete(modifiedAssemblyCSharp);
            }

            using (ModuleDefMD module = ModuleDefMD.Load(assemblyCSharp))
            using (ModuleDefMD NfhcPatcherAssembly = ModuleDefMD.Load(NfhcPatcherPath))
            {
                TypeDef NfhcMainDefinition = NfhcPatcherAssembly.GetTypes().FirstOrDefault(x => x.Name == PoCoopENTRY_TYPE_NAME);
                MethodDef executeMethodDefinition = NfhcMainDefinition.Methods.FirstOrDefault(x => x.Name == PoCoopENTRY_METHOD_NAME);

                MemberRef executeMethodReference = module.Import(executeMethodDefinition);

                TypeDef gameInputType = module.GetTypes().First(x => x.FullName == GAME_INPUT_TYPE_NAME);
                MethodDef awakeMethod = gameInputType.Methods.First(x => x.Name == GAME_INPUT_METHOD_NAME);

                Instruction callNfhcExecuteInstruction = OpCodes.Call.ToInstruction(executeMethodReference);

                awakeMethod.Body.Instructions.Insert(0, callNfhcExecuteInstruction);
                module.Write(modifiedAssemblyCSharp);
            }

            // The assembly might be used by other code or some other program might work in it. Retry to be on the safe side.
            Exception error = RetryWait(() => File.Delete(assemblyCSharp), 100, 5);
            if (error != null)
            {
                throw error;
            }
            File.Move(modifiedAssemblyCSharp, assemblyCSharp);
        }

        private Exception RetryWait(Action action, int interval, int retries = 0)
        {
            Exception lastException = null;
            while (retries >= 0)
            {
                try
                {
                    retries--;
                    action();
                    return null;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    Task.Delay(interval).Wait();
                }
            }
            return lastException;
        }

        public void Remove()
        {
            string assemblyCSharp = Path.Combine(NfhcManagedPath, GAME_ASSEMBLY_NAME);
            string modifiedAssemblyCSharp = Path.Combine(NfhcManagedPath, GAME_ASSEMBLY_MODIFIED_NAME);

            using (ModuleDefMD module = ModuleDefMD.Load(assemblyCSharp))
            {
                var moduleTypes = module.GetTypes();

                TypeDef gameInputType = moduleTypes.First(x => x.FullName == GAME_INPUT_TYPE_NAME);
                MethodDef awakeMethod = gameInputType.Methods.First(x => x.Name == GAME_INPUT_METHOD_NAME);

                IList<Instruction> methodInstructions = awakeMethod.Body.Instructions;
                int NfhcExecuteInstructionIndex = FindNfhcExecuteInstructionIndex(methodInstructions);

                if (NfhcExecuteInstructionIndex == -1)
                {
                    return;
                }

                methodInstructions.RemoveAt(NfhcExecuteInstructionIndex);
                module.Write(modifiedAssemblyCSharp);

                File.SetAttributes(assemblyCSharp, System.IO.FileAttributes.Normal);
            }

            File.Delete(assemblyCSharp);
            File.Move(modifiedAssemblyCSharp, assemblyCSharp);
        }

        private static int FindNfhcExecuteInstructionIndex(IList<Instruction> methodInstructions)
        {
            for (int instructionIndex = 0; instructionIndex < methodInstructions.Count; instructionIndex++)
            {
                string instruction = methodInstructions[instructionIndex].Operand?.ToString();

                if (instruction == PoCoop_EXECUTE_INSTRUCTION)
                {
                    return instructionIndex;
                }
            }

            return -1;
        }

        private bool IsPatchApplied()
        {
            string gameInputPath = Path.Combine(NfhcManagedPath, GAME_ASSEMBLY_NAME);

            using (ModuleDefMD module = ModuleDefMD.Load(gameInputPath))
            {
                TypeDef gameInputType = module.GetTypes().First(x => x.FullName == GAME_INPUT_TYPE_NAME);
                MethodDef awakeMethod = gameInputType.Methods.First(x => x.Name == GAME_INPUT_METHOD_NAME);

                return awakeMethod.Body.Instructions.Any(instruction => instruction.Operand?.ToString() == PoCoop_EXECUTE_INSTRUCTION);
            }
        }
    }
}
