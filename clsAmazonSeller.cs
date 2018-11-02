namespace amazon.seller.crawler
{
    class AmazonSeller
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public double Rating30DaysPercentage { get; set; } = 0.0;
        public double Rating30DaysStars
        {
            get
            {
                return (this.Rating30DaysPercentage / 100) * 5;
            }
        }
        public string Rating30DaysStarsSymbol
        {
            get
            {
                return this.getStars(this.Rating30DaysStars);
            }
        }
        public double Rating90DaysPercentage { get; set; } = 0.0;
        public double Rating90DaysStars
        {
            get
            {
                return (this.Rating90DaysPercentage / 100) * 5;
            }
        }
        public string Rating90DaysStarsSymbol
        {
            get
            {
                return this.getStars(this.Rating90DaysStars);
            }
        }
        public double Rating12MonthPercentage { get; set; } = 0.0;
        public double Rating12MonthStars
        {
            get
            {
                return (this.Rating12MonthPercentage / 100) * 5;
            }
        }
        public string Rating12MonthStarsSymbol
        {
            get
            {
                return this.getStars(this.Rating12MonthStars);
            }
        }
        public double RatingTotalPercentage { get; set; } = 0.0;
        public double RatingTotalStars
        {
            get
            {
                return (this.RatingTotalPercentage / 100) * 5;
            }
        }
        public string RatingTotalStarsSymbol
        {
            get
            {
                return this.getStars(this.RatingTotalStars);
            }
        }

        private string getStars(double nr)
        {
            try
            {
                string result = string.Empty;
                int a = System.Convert.ToInt16(nr);
                for (int i = 0; i < a; i++)
                {
                    result += '*';
                }
                return result;
            }
            catch (System.Exception)
            {
                return string.Empty;
            }
        }
    }
}