namespace Data
{
    public static class SD
    {
        public const string God = "God";
        public const string Admin = "Admin";
        public const string User = "User";
        public const string Moderator = "Moderator";
        public const string TeamLead = "TeamLead";

        public const string SenderEmail = "";
        public enum ApiType
        {
            GET,
            POST,
            PUT,
            DELETE
        }

        public const string BasicPlanCode = "Basic";
        public const string StarterPlanCode = "Starter";
        public const string ProPlanCode = "Pro";
        public const string BusinessPlanCode = "Business";
        public const string EnterprisePlanCode = "Enterprise";


        public const string ExpertPlanCode = "Expert";

        public const string TariffPlanSource = "SaaSPirate";

        public static class SaaSPirate
        {
			public const string BasicPlanCode = "BasicSaaSPirate";
			public const string StarterPlanCode = "StarterSaaSPirate";
			public const string ProPlanCode = "ProSaaSPirate";
			public const string BusinessPlanCode = "BusinessSaaSPirate";
			public const string EnterprisePlanCode = "EnterpriseSaaSPirate";
		}

		public static class Gumroad
        {
            public const string ProductCode = "fxomj";
            public const string StarterVariant = "Starter";
            public const string ProVariant = "Pro";
            public const string BusinessVariant = "Business";
            public const string EnterpriseVariant = "Enterprise";

            public enum ProductType
            {
                BasicPlan,
                StarterPlan,
                ProPlan,
                BusinessPlan,
                EnterprisePlan,
                ExpertPlan
            }
        }

	}
}
