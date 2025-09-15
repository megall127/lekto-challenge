using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace erp_server.Attributes
{
    public class TelefoneValidoAttribute : ValidationAttribute
    {
        public TelefoneValidoAttribute() : base("Telefone deve ter um formato válido com DDD")
        {
        }

        public override bool IsValid(object? value)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return true; 

            var telefone = Regex.Replace(value.ToString()!, @"\D", ""); 
            

            if (telefone.Length != 10 && telefone.Length != 11)
                return false;

          
            if (!int.TryParse(telefone.Substring(0, 2), out int ddd))
                return false;

            var dddsValidos = new int[]
            {
                11, 12, 13, 14, 15, 16, 17, 18, 19, // SP
                21, 22, 24, // RJ/ES
                27, 28, // ES
                31, 32, 33, 34, 35, 37, 38, // MG
                41, 42, 43, 44, 45, 46, // PR
                47, 48, 49, // SC
                51, 53, 54, 55, // RS
                61, // DF/GO
                62, 64, // GO
                63, // TO
                65, 66, // MT
                67, // MS
                68, // AC
                69, // RO
                71, 73, 74, 75, 77, // BA
                79, // SE
                81, 87, // PE
                82, // AL
                83, // PB
                84, // RN
                85, 88, // CE
                86, 89, // PI
                91, 93, 94, // PA
                92, 97, // AM
                95, // RR
                96, // AP
                98, 99 // MA
            };

            if (!dddsValidos.Contains(ddd))
                return false;

            // Para números de 11 dígitos, o primeiro dígito após o DDD deve ser 9 (celular)
            if (telefone.Length == 11)
            {
                if (!int.TryParse(telefone.Substring(2, 1), out int primeiroDigito) || primeiroDigito != 9)
                    return false;
            }

            // Para números de 10 dígitos, o primeiro dígito após o DDD deve ser 2-5 (fixo)
            if (telefone.Length == 10)
            {
                if (!int.TryParse(telefone.Substring(2, 1), out int primeiroDigito) || 
                    primeiroDigito < 2 || primeiroDigito > 5)
                    return false;
            }

            return true;
        }
    }
}
