using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

// Load Template
// Useful functions (parse_feet, string_contains_word)
// Compile during test
// Test w/o debug

namespace XPath_Template
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            cb_assigned_to.SelectedIndex = 0;
            cb_status.SelectedIndex = 0;
        }

        // returns true if 1-or-more input fields are empty, otherwise returns false
        private bool no_fields_are_empty()
        {
            if (tb_spider_denomer.TextLength < 3 |
                tb_domain.TextLength < 3 |
                tb_url.TextLength < 3 |
                tb_boat_listings.TextLength < 3 |
                tb_next_page.TextLength < 3 |
                tb_specifications.TextLength < 3 |
                tb_boat_make.TextLength < 3 |
                tb_boat_model.TextLength < 3 |
                tb_boat_year.TextLength < 3 |
                tb_boat_condition.TextLength < 3 |
                tb_boat_price.TextLength < 3 |
                tb_boat_length.TextLength < 3 |
                tb_boat_material.TextLength < 3 |
                tb_boat_location.TextLength < 3 |
                tb_boat_country.TextLength < 3 |
                cb_assigned_to.Text.Length < 3 |
                cb_status.Text.Length < 3)
                //rtb_notes.Text.Trim().Length < 3)
            { return false; }
            return true;
        }

        private void btn_Create_Click(object sender, EventArgs e)
        {
            create_template();
        }
        private void btn_test_Click(object sender, EventArgs e)
        {
            create_template();// ensure template is compiled before running
            crawl_spider('c');
        }

        private void cb_parse_gbp_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_parse_gbp.Checked) { cb_startchar.Enabled = true; }
            else { cb_startchar.Enabled = false; }
        }
        private void cb_parse_feet_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_parse_feet.Checked) { cb_convert_metres_to_feet.Enabled = true; }
            else { cb_convert_metres_to_feet.Enabled = false; }
        }

        private void btn_debug_Click(object sender, EventArgs e)
        {
            create_template("pass#");// overwrite template to temporary put it into Debug Mode
            crawl_spider();// run scrapy on temporary template (no pagination)
            create_template();// reset template back to original
        }
        private void btn_csv_Click(object sender, EventArgs e)
        {
            crawl_spider('n');
        }

        private void tb_spider_denomer_TextChanged(object sender, EventArgs e)
        {
            // only proceed if on Load Template...
            if (btn_load_template.Text == "Clear Template!") { return; }

            // grab source directory from the txt file
            string folder_path = "";
            try { folder_path = File.ReadAllText(@"./filepath.txt"); }
            catch { error_box("Could not find filepath.txt?"); return; }

            // grab filepath from below the source directory using the spider name:
            string file_path = $@"{folder_path}\best_scraper\spiders\{tb_spider_denomer.Text.ToLower()}.py";

            // if file exists, enable Load Template button - otherwise disable it
            if (File.Exists(file_path)) { btn_load_template.Enabled = true; }
            else { btn_load_template.Enabled = false; }

            //if (tb_spider_denomer.TextLength < 3 & btn_load_template.Text=="Load Template...") {  }
            //else {  }
        }
        private void btn_load_template_Click(object sender, EventArgs e)
        {
            if (btn_load_template.Text == "Clear Template!") { clear_template(); }
            else { load_template(); }
        }

        // Custom Functions:
        private void error_box(string msg)
        {
            MessageBox.Show(
            msg,
            "Error!",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error
            );
        }
        private string yield_substring(decimal a, decimal b, string sub = "UNKNOWN")
        {
            // determine if any substring suffixes to each specification exist and append them if so:
            if (a != 0 | b != 0)
            {
                if (a > 0) { for (int i = 0; i < a; i++) { sub = 'x' + sub; } }
                if (b < 0) { for (int i = 0; i < b; i++) { sub = sub + 'x'; } }
                sub = sub + $"')[{a}:{b}]".Replace("[0:", "[:").Replace(":0]", ":]");
            }
            else { sub += "')"; }
            return sub;
        }
        private void create_template(string debugmode="")//debugmode="pass#" if Debug Mode is ON
        {
            // replace apostrophes and blank edges:
            tb_spider_denomer.Text = tb_spider_denomer.Text.Trim();
            tb_domain.Text = tb_domain.Text.Trim();
            tb_url.Text = tb_url.Text.Replace("\"", "'").Trim();
            tb_boat_listings.Text = tb_boat_listings.Text.Replace("'", "\"").Trim();
            tb_next_page.Text = tb_next_page.Text.Replace("'", "\"").Trim();
            tb_specifications.Text = tb_specifications.Text.Replace("'", "\"").Trim();
            tb_boat_make.Text = tb_boat_make.Text.Replace("'", "\"").Trim();
            tb_boat_model.Text = tb_boat_model.Text.Replace("'", "\"").Trim();
            tb_boat_year.Text = tb_boat_year.Text.Replace("'", "\"").Trim();
            tb_boat_condition.Text = tb_boat_condition.Text.Replace("'", "\"").Trim();
            tb_boat_price.Text = tb_boat_price.Text.Replace("'", "\"").Trim();
            tb_boat_length.Text = tb_boat_length.Text.Replace("'", "\"").Trim();
            tb_boat_material.Text = tb_boat_material.Text.Replace("'", "\"").Trim();
            tb_boat_location.Text = tb_boat_location.Text.Replace("'", "\"").Trim();
            tb_boat_country.Text = tb_boat_country.Text.Replace("'", "\"").Trim();

            if (no_fields_are_empty())//all form fields filled with something:
            {
                // metadata:
                string assigned_to = cb_assigned_to.Text;
                string brief_status = cb_status.Text;
                string notes = rtb_notes.Text;
                if (notes != "") { notes = "\n"+notes; }

                // determine what fields can have the word "sold" in them (for removal purposes):
                string sold_make = "";
                if (cb_sold_make.Checked) { sold_make = "\t\tif string_contains_word(specifications['make']):yield None\n"; }
                string sold_model = "";
                if (cb_sold_model.Checked) { sold_model = "\t\tif string_contains_word(specifications['model']):yield None\n"; }
                string sold_year = "";
                if (cb_sold_year.Checked) { sold_year = "\t\tif string_contains_word(specifications['year']):yield None\n"; }
                string sold_condition = "";
                if (cb_sold_condition.Checked) { sold_condition = "\t\tif string_contains_word(specifications['condition']):yield None\n"; }
                string sold_price = "";
                if (cb_sold_price.Checked) { sold_price = "\t\tif string_contains_word(specifications['price']):yield None\n"; }
                string sold_material = "";
                if (cb_sold_material.Checked) { sold_material = "\t\tif string_contains_word(specifications['hull_material']):yield None\n"; }
                string sold_location = "";
                if (cb_sold_location.Checked) { sold_location = "\t\tif string_contains_word(specifications['location']):yield None\n"; }
                string sold_country = "";
                if (cb_sold_country.Checked) { sold_country = "\t\tif string_contains_word(specifications['country']):yield None\n"; }
                string sold = sold_make + sold_model + sold_year + sold_condition + sold_price + sold_material + sold_location + sold_country;
                if (sold!="") { sold = "\n\t\t# Remove sold boats:\n" + sold + "\n"; }

                // infinite scroll stuff:
                string infinite_scroll_headers = "";
                if (cb_infinite_scroll.Checked) { infinite_scroll_headers = "\tpage_number=1\n"; } // only 1 header for now
                string next_page_link_a = $"\t\tnext_page = response.xpath('{tb_next_page.Text}/@href').get(default=[])\n";
                string next_page_link_b = "\t\tif next_page:# if a next page button exists, click it!\n";
                string next_page_link_c = $"\t\t\t{debugmode}yield scrapy.Request(url=response.urljoin(next_page), callback=self.parse)\n";
                if (cb_infinite_scroll.Checked)
                { 
                    next_page_link_a = "\t\tif boat_listing != None:#if not the end of the infinite scroll:\n";
                    next_page_link_b = "\t\t\tself.page_number+=1\n";
                    next_page_link_c = $"\t\t\t{debugmode}yield scrapy.Request(url=f'{tb_next_page.Text}', callback=self.parse)\n";
                }

                // determine if specifications come from the specs_table or not:
                string make_source = "specs_table";
                if (tb_boat_make.Text.Replace("(","").Substring(0, 2) == "//") { make_source = "response"; }
                string model_source = "specs_table";
                if (tb_boat_model.Text.Replace("(", "").Substring(0, 2) == "//") { model_source = "response"; }
                string year_source = "specs_table";
                if (tb_boat_year.Text.Replace("(", "").Substring(0, 2) == "//") { year_source = "response"; }
                string condition_source = "specs_table";
                if (tb_boat_condition.Text.Replace("(", "").Substring(0, 2) == "//") { condition_source = "response"; }
                string price_source = "specs_table";
                if (tb_boat_price.Text.Replace("(", "").Substring(0, 2) == "//") { price_source = "response"; }
                string length_source = "specs_table";
                if (tb_boat_length.Text.Replace("(", "").Substring(0, 2) == "//") { length_source = "response"; }
                string material_source = "specs_table";
                if (tb_boat_material.Text.Replace("(", "").Substring(0, 2) == "//") { material_source = "response"; }
                string location_source = "specs_table";
                if (tb_boat_location.Text.Replace("(", "").Substring(0, 2) == "//") { location_source = "response"; }
                string country_source = "specs_table";
                if (tb_boat_country.Text.Replace("(", "").Substring(0, 2) == "//") { country_source = "response"; }

                // check for text() and @href suffixes?
                // <TBC>

                // determine if any substring suffixes to each specification exist and append them if so:
                string make_sub = yield_substring(ud_make_a.Value,ud_make_b.Value);
                string model_sub = yield_substring(ud_model_a.Value, ud_model_b.Value);
                string year_sub = yield_substring(ud_year_a.Value, ud_year_a.Value);
                string condition_sub = yield_substring(ud_condition_a.Value, ud_condition_a.Value);
                string price_sub = yield_substring(ud_price_a.Value, ud_price_b.Value, "0");
                string length_sub = yield_substring(ud_length_a.Value, ud_length_b.Value, "0");
                string material_sub = yield_substring(ud_material_a.Value, ud_material_b.Value);
                string location_sub = yield_substring(ud_location_a.Value, ud_location_b.Value);
                string country_sub = yield_substring(ud_country_a.Value, ud_country_b.Value);

                // post-processing handling:
                string absolute_url = "";
                if (cb_absolute_url.Checked) { absolute_url = "\t\t\thref = response.urljoin(href)# combine domain with href to form a full absolute_url\n"; }
                string parse_feet = "";
                if (cb_parse_feet.Checked == true) 
                {
                    char foot_mode = 'f';
                    if (cb_convert_metres_to_feet.Checked) { foot_mode = 'm'; }
                    parse_feet = $"\t\tspecifications['length'] = parse_feet(specifications['length'],'{foot_mode}')\n";
                }
                //string convert_metres_to_feet = "";
                //if (cb_convert_metres_to_feet.Checked) { convert_metres_to_feet = "\t\tspecifications['length'] = convert_metres_to_feet(specifications['length'])\n"; }
                string parse_gbp = "";
                if (cb_parse_gbp.Checked == true) { parse_gbp = $"\t\tspecifications['price'] = parse_gbp(specifications['price'],'{cb_startchar.Text}')\n"; }
                string post_processing = "";
                if (parse_feet != "" | parse_gbp != "" | sold != "") { post_processing = "\t\t# Post-Processing Begins:\n"; }

                string file_content = "import scrapy\n" +
"from ._useful_functions import *\n" +
"\n\"\"\"\n" +
$"Assigned To: {assigned_to}\nBrief Status: {brief_status}{notes}"+//metadata
"\n\"\"\"\n\n" +
$"class {CultureInfo.CurrentCulture.TextInfo.ToTitleCase(tb_spider_denomer.Text.ToLower())}Spider(scrapy.Spider):\n" +
$"\tname = '{tb_spider_denomer.Text.ToLower()}'\n" +
$"\tallowed_domains = ['{tb_domain.Text}']\n" +
$"{infinite_scroll_headers}" +//only appear if required
$"\tstart_urls = [f'{tb_url.Text}']\n" +
"\n" +
"\tdef parse(self, response):\n" +
"\n" +
"\t\t# Grab details on every Boat Listing from this web page's response:\n" +
$"\t\tfor boat_listing in response.xpath('{tb_boat_listings.Text}/@href'):\n" +
"\n" +
"\t\t\t# grab the boat_listing's full URL:\n" +
"\t\t\thref = boat_listing.get()# get just the href without the array stuff\n" +
$"{absolute_url}" +// appends href to domain url if the href is not a full url like "/boat-123"
"\n" +
"\t\t\t# run parse_boat(href) and return the boat listing:\n" +
"\t\t\tspecifications = response.follow(url=href, callback=self.parse_boat,meta={'URL':href})\n" +
"\t\t\tif not specifications:\n" +
"\t\t\t\tcontinue# if specifications returns as False, continue to next boat because that means there is a need to remove it from results!\n" +
"\t\t\tyield specifications# otherwise, return it as is!\n" +
"\n" +
"\t\t# Pagination Begins:\n" +
$"{next_page_link_a}{next_page_link_b}{next_page_link_c}"+
"\n" +
"\tdef parse_boat(self,response):\n" +
"\n" +
"\t\t# Define the full-formed url of the boat listing as a value from the parse() that called this function:\n" +
"\t\thref = response.request.meta['URL']\n" +
"\n" +
"\t\t# Define and get what specifications we desire:\n" +
$"\t\tspecs_table = response.xpath('{tb_specifications.Text}')\n" +
"\t\tspecifications = {\n" +
$"\t\t\t'make':{make_source}.xpath('{tb_boat_make.Text}').get(default='{make_sub},\n" +
$"\t\t\t'model':{model_source}.xpath('{tb_boat_model.Text}').get(default='{model_sub},\n" +
$"\t\t\t'year':{year_source}.xpath('{tb_boat_year.Text}').get(default='{year_sub},\n" +
$"\t\t\t'condition':{condition_source}.xpath('{tb_boat_condition.Text}').get(default='{condition_sub},\n" +
$"\t\t\t'price':{price_source}.xpath('{tb_boat_price.Text}').get(default='{price_sub},\n" +
$"\t\t\t'length':{length_source}.xpath('{tb_boat_length.Text}').get(default='{length_sub},\n" +
$"\t\t\t'hull_material':{material_source}.xpath('{tb_boat_material.Text}').get(default='{material_sub},\n" +
$"\t\t\t'location':{location_source}.xpath('{tb_boat_location.Text}').get(default='{location_sub},\n" +
$"\t\t\t'country':{country_source}.xpath('{tb_boat_country.Text}').get(default='{country_sub},\n" +
"\t\t\t'url':href.split('?')[0]}# remove any trailing query to save on memory space and prevent duplicates\n" +
"\n" +
$"{post_processing}{sold}{parse_feet}{parse_gbp}" +//convert_metres_to_feet
"\n" +
"\t\tyield specifications";

                // grab source directory from the txt file
                string folder_path = "";
                try { folder_path = File.ReadAllText(@"./filepath.txt"); }
                catch (IOException) { error_box("Please close filepath.txt before running this!"); return; }

                string file_path = $@"{folder_path}\best_scraper\spiders\{tb_spider_denomer.Text.ToLower()}.py";

                try { File.WriteAllText(file_path, file_content); }
                catch (IOException) { error_box($"Please close {file_path} before running this!"); return; }
            }
            else//at least one field has not been filled in:
            {
                error_box("Please ensure every field has an input before submitting!");
            }
        }
        private void load_template()
        {
            // grab source directory from the txt file
            string folder_path = "";
            try { folder_path = File.ReadAllText(@"./filepath.txt"); }
            catch { error_box("Could not find filepath.txt?"); return; }

            // grab filepath from below the source directory using the spider name:
            string file_path = $@"{folder_path}\best_scraper\spiders\{tb_spider_denomer.Text.ToLower()}.py";
            if (File.Exists(file_path) == false)
            { error_box($"Could not find {tb_spider_denomer.Text}.py!\n\nPlease enter an existing Spider Name!"); return; }

            // get every line from above file in an array:
            string[] lines = new string[] { };
            try { lines = File.ReadAllLines(file_path); }
            catch { error_box($"Could not read {tb_spider_denomer.Text}.py?"); return; }
            // If data found, Spider Name is legit so it remains unchanged

            // blank values first (false to exclude spider denomer):
            clear_template(false);

            // read remaining desired data from lines:
            int next = -2;
            int find = -1;
            string tail = "";
            string[] split;
            foreach (string l in lines)
            {
                switch (next)
                {
                    case -2:
                        find = l.IndexOf("Assigned");
                        if (find > -1) { find = l.IndexOf(": ") + 2; cb_assigned_to.SelectedItem = l.Substring(find); next += 1; }
                        continue;
                    case -1:
                        find = l.IndexOf("Brief");
                        if (find > -1) { find = l.IndexOf(": ") + 2; cb_status.SelectedItem = l.Substring(find); next += 1; }
                        continue;
                    case 0:
                        find = l.IndexOf("\"\"\"");
                        if (find > -1) { if (rtb_notes.Text.Length > 0) { rtb_notes.Text = rtb_notes.Text.Substring(1); } next += 1; }
                        else { rtb_notes.Text += "\n" + l; }
                        continue;
                    case 1:
                        find = l.IndexOf("allowed_domains");
                        if (find > -1) { find = l.IndexOf('[') + 2; tb_domain.Text = l.Substring(find, l.IndexOf(']') - find - 1); next += 1; }
                        continue;
                    case 2:
                        find = l.IndexOf("start_urls");
                        if (find > -1) { find = l.IndexOf('[') + 3; tb_url.Text = l.Substring(find, l.IndexOf(']') - find - 1); next += 1; }
                        continue;
                    case 3:
                        find = l.IndexOf("for boat_listing");
                        if (find > -1) { find = l.IndexOf('(') + 2; tb_boat_listings.Text = l.Substring(find, l.IndexOf("):") - find - 7); next += 1; }
                        continue;
                    case 4:
                        find = l.IndexOf("urljoin");
                        if (find > -1) { cb_absolute_url.Checked = true; next += 1; }
                        else { if (l.IndexOf("follow") > -1) { next += 1; } }// no urljoin :(
                        continue;
                    case 5:
                        find = l.IndexOf("next_page");
                        if (find > -1) { find = l.IndexOf('(') + 2; tb_next_page.Text = l.Substring(find, l.IndexOf(").") - find - 7); next += 1; }
                        else//infinite scroll stuff:
                        {
                            find = l.IndexOf("url=f'");
                            if (find > -1) 
                            { 
                                tb_next_page.Text = l.Substring(find + 6, l.IndexOf("', c") - find - 6);
                                cb_infinite_scroll.Checked = true;
                                next += 1; 
                            }
                        }
                        continue;
                    case 6:
                        find = l.IndexOf("specs_table =");
                        if (find > -1) { find = l.IndexOf('(') + 2; tb_specifications.Text = l.Substring(find, l.IndexOf("')") - find); next += 1; }
                        continue;
                    case 7:
                        find = l.IndexOf("make");
                        if (find > -1)
                        { 
                            find = l.IndexOf('(') + 2;
                            tb_boat_make.Text = l.Substring(find, l.IndexOf("').") - find);
                            tail = l.Substring(l.IndexOf("')[") + 3);
                            tail = tail.Remove(tail.Length - 2);
                            split = tail.Split(':');
                            ud_make_a.Value = decimal.TryParse(split[0], out decimal a) ? decimal.Parse(split[0]) : 0;
                            ud_make_b.Value = decimal.TryParse(split[1], out decimal b) ? decimal.Parse(split[1]) : 0;
                            next += 1; 
                        }
                        continue;
                    case 8:
                        find = l.IndexOf("model");
                        if (find > -1)
                        {
                            find = l.IndexOf('(') + 2;
                            tb_boat_model.Text = l.Substring(find, l.IndexOf("').") - find);
                            tail = l.Substring(l.IndexOf("')[") + 3);
                            tail = tail.Remove(tail.Length - 2);
                            split = tail.Split(':');
                            ud_model_a.Value = decimal.TryParse(split[0], out decimal a) ? decimal.Parse(split[0]) : 0;
                            ud_model_b.Value = decimal.TryParse(split[1], out decimal b) ? decimal.Parse(split[1]) : 0;
                            next += 1;
                        }
                        continue;
                    case 9:
                        find = l.IndexOf("year");
                        if (find > -1)
                        {
                            find = l.IndexOf('(') + 2;
                            tb_boat_year.Text = l.Substring(find, l.IndexOf("').") - find);
                            tail = l.Substring(l.IndexOf("')[") + 3);
                            tail = tail.Remove(tail.Length - 2);
                            split = tail.Split(':');
                            ud_year_a.Value = decimal.TryParse(split[0], out decimal a) ? decimal.Parse(split[0]) : 0;
                            ud_year_b.Value = decimal.TryParse(split[1], out decimal b) ? decimal.Parse(split[1]) : 0;
                            next += 1;
                        }
                        continue;
                    case 10:
                        find = l.IndexOf("condition");
                        if (find > -1)
                        {
                            find = l.IndexOf('(') + 2;
                            tb_boat_condition.Text = l.Substring(find, l.IndexOf("').") - find);
                            tail = l.Substring(l.IndexOf("')[") + 3);
                            tail = tail.Remove(tail.Length - 2);
                            split = tail.Split(':');
                            ud_condition_a.Value = decimal.TryParse(split[0], out decimal a) ? decimal.Parse(split[0]) : 0;
                            ud_condition_b.Value = decimal.TryParse(split[1], out decimal b) ? decimal.Parse(split[1]) : 0;
                            next += 1;
                        }
                        continue;
                    case 11:
                        find = l.IndexOf("price");
                        if (find > -1)
                        {
                            find = l.IndexOf('(') + 2;
                            tb_boat_price.Text = l.Substring(find, l.IndexOf("').") - find);
                            tail = l.Substring(l.IndexOf("')[") + 3);
                            tail = tail.Remove(tail.Length - 2);
                            split = tail.Split(':');
                            ud_price_a.Value = decimal.TryParse(split[0], out decimal a) ? decimal.Parse(split[0]) : 0;
                            ud_price_b.Value = decimal.TryParse(split[1], out decimal b) ? decimal.Parse(split[1]) : 0;
                            next += 1;
                        }
                        continue;
                    case 12:
                        find = l.IndexOf("length");
                        if (find > -1)
                        {
                            find = l.IndexOf('(') + 2;
                            tb_boat_length.Text = l.Substring(find, l.IndexOf("').") - find);
                            tail = l.Substring(l.IndexOf("')[") + 3);
                            tail = tail.Remove(tail.Length - 2);
                            split = tail.Split(':');
                            ud_length_a.Value = decimal.TryParse(split[0], out decimal a) ? decimal.Parse(split[0]) : 0;
                            ud_length_b.Value = decimal.TryParse(split[1], out decimal b) ? decimal.Parse(split[1]) : 0;
                            next += 1;
                        }
                        continue;
                    case 13:
                        find = l.IndexOf("hull_material");
                        if (find > -1)
                        {
                            find = l.IndexOf('(') + 2;
                            tb_boat_material.Text = l.Substring(find, l.IndexOf("').") - find);
                            tail = l.Substring(l.IndexOf("')[") + 3);
                            tail = tail.Remove(tail.Length - 2);
                            split = tail.Split(':');
                            ud_material_a.Value = decimal.TryParse(split[0], out decimal a) ? decimal.Parse(split[0]) : 0;
                            ud_material_b.Value = decimal.TryParse(split[1], out decimal b) ? decimal.Parse(split[1]) : 0;
                            next += 1;
                        }
                        continue;
                    case 14:
                        find = l.IndexOf("location");
                        if (find > -1)
                        {
                            find = l.IndexOf('(') + 2;
                            tb_boat_location.Text = l.Substring(find, l.IndexOf("').") - find);
                            tail = l.Substring(l.IndexOf("')[") + 3);
                            tail = tail.Remove(tail.Length - 2);
                            split = tail.Split(':');
                            ud_location_a.Value = decimal.TryParse(split[0], out decimal a) ? decimal.Parse(split[0]) : 0;
                            ud_location_b.Value = decimal.TryParse(split[1], out decimal b) ? decimal.Parse(split[1]) : 0;
                            next += 1;
                        }
                        continue;
                    case 15:
                        find = l.IndexOf("country");
                        if (find > -1)
                        {
                            find = l.IndexOf('(') + 2;
                            tb_boat_country.Text = l.Substring(find, l.IndexOf("').") - find);
                            tail = l.Substring(l.IndexOf("')[") + 3);
                            tail = tail.Remove(tail.Length - 2);
                            split = tail.Split(':');
                            ud_country_a.Value = decimal.TryParse(split[0], out decimal a) ? decimal.Parse(split[0]) : 0;
                            ud_country_b.Value = decimal.TryParse(split[1], out decimal b) ? decimal.Parse(split[1]) : 0;
                            next += 1;
                        }
                        continue;
                    case 16://post-processing
                        find = l.IndexOf('(');
                        if (find > -1)
                        {
                            if (l.IndexOf("string_contains_word") > -1)
                            {
                                if (l.IndexOf("make") > -1) { cb_sold_make.Checked = true; }
                                else if (l.IndexOf("model") > -1) { cb_sold_model.Checked = true; }
                                else if (l.IndexOf("year") > -1) { cb_sold_year.Checked = true; }
                                else if (l.IndexOf("condition") > -1) { cb_sold_condition.Checked = true; }
                                else if (l.IndexOf("price") > -1) { cb_sold_price.Checked = true; }
                                else if (l.IndexOf("length") > -1) { cb_sold_length.Checked = true; }
                                else if (l.IndexOf("hull_material") > -1) { cb_sold_material.Checked = true; }
                                else if (l.IndexOf("location") > -1) { cb_sold_location.Checked = true; }
                                else if (l.IndexOf("country") > -1) { cb_sold_country.Checked = true; }
                            }
                            if (l.IndexOf("parse_gbp") > -1)
                            {
                                cb_parse_gbp.Checked = true;
                                cb_startchar.Text = l.Substring(l.IndexOf(",'") + 2, 1);
                            }
                            else if (l.IndexOf("parse_feet") > -1)
                            {
                                cb_parse_feet.Checked = true;
                                if (l.IndexOf("'m'") > -1) { cb_convert_metres_to_feet.Checked = true; }
                            }
                        }
                        continue;
                    default:
                        break;
                }
            }
            btn_load_template.Text = "Clear Template!";
        }
        private void clear_template(bool includespider=true)
        {
            tb_domain.Text = "";
            tb_url.Text = "";
            tb_boat_listings.Text = "";
            tb_next_page.Text = "";
            tb_specifications.Text = "";
            tb_boat_make.Text = "";
            tb_boat_model.Text = "";
            tb_boat_year.Text = "";
            tb_boat_condition.Text = "";
            tb_boat_price.Text = "";
            tb_boat_length.Text = "";
            tb_boat_material.Text = "";
            tb_boat_location.Text = "";
            tb_boat_country.Text = "";
            rtb_notes.Text = "";

            ud_make_a.Value = 0;
            ud_make_b.Value = 0;
            ud_model_a.Value = 0;
            ud_model_b.Value = 0;
            ud_year_a.Value = 0;
            ud_year_b.Value = 0;
            ud_condition_a.Value = 0;
            ud_condition_b.Value = 0;
            ud_price_a.Value = 0;
            ud_price_b.Value = 0;
            ud_length_a.Value = 0;
            ud_length_b.Value = 0;
            ud_material_a.Value = 0;
            ud_material_b.Value = 0;
            ud_location_a.Value = 0;
            ud_location_b.Value = 0;
            ud_country_a.Value = 0;
            ud_country_b.Value = 0;

            cb_status.SelectedIndex = 0;

            cb_sold_make.Checked = false;
            cb_sold_model.Checked = false;
            cb_sold_year.Checked = false;
            cb_sold_condition.Checked = false;
            cb_sold_price.Checked = false;
            cb_sold_length.Checked = false;
            cb_sold_material.Checked = false;
            cb_sold_location.Checked = false;
            cb_sold_country.Checked = false;

            cb_absolute_url.Checked = false;
            cb_infinite_scroll.Checked = false;
            cb_convert_metres_to_feet.Checked = false;
            cb_parse_feet.Checked = false;
            cb_parse_gbp.Checked = false;
            cb_startchar.Text = "£";

            if (includespider) 
            { 
                tb_spider_denomer.Text = "";
                btn_load_template.Text = "Load Template...";
                btn_load_template.Enabled = false;
            }
        }
        private void crawl_spider(char debug = 'k')//debug='c' when not debugging cmd
        {
            // grab root scrapy project directory from the txt file
            string folder_path = "";
            try { folder_path = File.ReadAllText(@"./filepath.txt"); }
            catch (IOException) { error_box("Please close filepath.txt before running this!"); return; }

            // clear csv first:
            try { File.Delete($@"{folder_path}\xpath_test.csv"); }
            catch (IOException) { error_box("Please close xpath_test.csv before running this!"); return; }
            

            // prepare to send scrapy cmd line input to user's root scrapy project directory
            ProcessStartInfo procStart =
                new ProcessStartInfo("cmd", $"/{debug} scrapy crawl {tb_spider_denomer.Text} -o xpath_test.csv");
            procStart.WorkingDirectory = folder_path;
            Process proc = new Process();
            proc.StartInfo = procStart;

            // run spider from cmd and wait for it to close:
            proc.Start();
            proc.WaitForExit();

            // open the csv to user:
            proc.StartInfo.FileName = "xpath_test.csv";
            if (File.Exists($@"{folder_path}\xpath_test.csv")) { proc.Start(); }
            //proc.WaitForExit();
        }
    }
}

// Boat Data
// future Boat Sellers