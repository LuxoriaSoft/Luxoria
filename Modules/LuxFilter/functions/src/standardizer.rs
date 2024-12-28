// LuxFilter standardizer module
// This module contains the standardizer struct and its implementation

// Normalize the data by mapping the values between 0 and 1
pub fn normalize(data: &[f64]) -> Vec<f64> {
    let max = data.iter().fold(f64::MIN, |a, &b| a.max(b));
    let min = data.iter().fold(f64::MAX, |a, &b| a.min(b));
    let range = max - min;

    if range == 0.0 {
        return vec![0.0; data.len()];
    }

    data.iter().map(|x| (x - min) / range).collect()
}

// Standardize the data by mapping the values to have a mean of 0 and a standard deviation of 1
pub fn standardize(data: &[f64]) -> Vec<f64> {
    let length = data.len();

    if length == 0 {
        return vec![];
    }

    let mean = data.iter().sum::<f64>() / length as f64;
    let variance = data.iter().map(|x| (x - mean).powi(2)).sum::<f64>() / length as f64;
    let std_dev = variance.sqrt();

    if std_dev == 0.0 {
        return vec![0.0; length];
    }

    data.iter().map(|x| (x - mean) / std_dev).collect()
}

#[cfg(test)]
mod tests {
    use super::*;

    // Normalize tests
    #[test]
    fn test_normalize_fnc_basic() {
        let data = vec![10.0, 20.0, 30.0, 40.0, 50.0];
        let result = normalize(&data);
        let expected = vec![0.0, 0.25, 0.5, 0.75, 1.0];

        assert_eq!(result, expected);
    }

    #[test]
    fn test_normalize_fnc_empty() {
        let data = vec![];
        let result = normalize(&data);
        let expected = vec![];

        assert_eq!(result, expected);
    }

    #[test]
    fn test_normalize_fnc_single() {
        let data = vec![10.0];
        let result = normalize(&data);
        let expected = vec![0.0];

        assert_eq!(result, expected);
    }

    #[test]
    fn test_normalize_fnc_same() {
        let data = vec![10.0, 10.0, 10.0, 10.0, 10.0];
        let result = normalize(&data);
        let expected = vec![0.0, 0.0, 0.0, 0.0, 0.0];

        assert_eq!(result, expected);
    }

    // Standardize tests
    #[test]
    fn test_standardize_fnc_basic() {
        let data = vec![10.0, 20.0, 30.0, 40.0, 50.0];
        let result = standardize(&data);
        let expected = vec![-1.414213562373095, -0.7071067811865475, 0.0, 0.7071067811865475, 1.414213562373095];

        assert_eq!(result, expected);
    }

    #[test]
    fn test_standardize_fnc_empty() {
        let data = vec![];
        let result = standardize(&data);
        let expected = vec![];

        assert_eq!(result, expected);
    }

    #[test]
    fn test_standardize_fnc_single() {
        let data = vec![10.0];
        let result = standardize(&data);
        let expected = vec![0.0];

        assert_eq!(result, expected);
    }

    #[test]
    fn test_standardize_fnc_same() {
        let data = vec![10.0, 10.0, 10.0, 10.0, 10.0];
        let result = standardize(&data);
        let expected = vec![0.0, 0.0, 0.0, 0.0, 0.0];

        assert_eq!(result, expected);
    }
}