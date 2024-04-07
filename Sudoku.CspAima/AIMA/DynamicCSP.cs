using System;
using aima.core.search.csp;

namespace Sudoku.CSPwithAIMA
{

    // Version customisée du CSP pour utiliser un constructeur vide, et fournissant le clone pour partager variables et une partie des contraintes
    public class DynamicCSP : CSP, ICloneable
    {

        public void AddNewVariable(Variable objVariable)
        {
            this.addVariable(objVariable);
        }


        /* copyDomains(): methods Returns a copy which contains a copy of
         * the domains list and is in all other aspects a flat copy of this.
         * */
        
        public object Clone()
        {
            var toReturn = new DynamicCSP();
            foreach (Variable variable in this.getVariables().toArray())
            {
                toReturn.addVariable(variable);
                toReturn.setDomain(variable, new Domain(this.getDomain(variable).asList()));
            }

            foreach (Constraint constraint in this.getConstraints().toArray())
            {
                toReturn.addConstraint(constraint);
            }

            return toReturn;
        }
    }


    


    //Contrainte AllDiff

    //public class AllDiffConstraint<TDomain> : Constraint
    //{

    //	public List<Variable> Variables  { get; set; }


    //	private java.util.List _Scope;

    //	public AllDiffConstraint(IEnumerable<Variable> objVars)
    //	{
    //		Variables = new List<Variable>(objVars);
    //		_Scope = new java.util.ArrayList(Variables.Count);
    //		Variables.ForEach(objVar => _Scope.add(objVar));
    //	}

    //       public java.util.List getScope()
    //       {
    //           return _Scope;
    //       }

    //       public bool isSatisfiedWith(Assignment a)
    //       {
    //           var objHashSet = new System.Collections.Generic.HashSet<TDomain>();
    //           foreach (var objVar in Variables)
    //           {
    //               var objValue = (TDomain)a.getAssignment(objVar);
    //               if (objHashSet.Contains(objValue))
    //               {
    //                   return false;
    //               }

    //               objHashSet.Add(objValue);
    //           }

    //           return true;
    //       }
    //   }




    // Contrainte en valeur pour le masque
    //public class ValueConstraint : Constraint
    //{

    //	public Variable Var1 { get; set; }

    //	public object Value { get; set; }

    //	private java.util.List _Scope;

    //	public ValueConstraint(Variable objVar1, object objValue)
    //	{
    //		Var1 = objVar1;
    //		Value = objValue;
    //		_Scope = new java.util.ArrayList(1);
    //		_Scope.add(Var1);
    //	}

    //	public java.util.List getScope()
    //	{
    //		return _Scope;
    //	}

    //	public bool isSatisfiedWith(Assignment a)
    //	{
    //		return a.getAssignment(Var1) == Value;
    //	}
    //}

}